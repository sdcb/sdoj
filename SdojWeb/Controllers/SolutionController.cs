﻿using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EntityFramework.Extensions;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Models.JudgePush;
using SdojWeb.SignalR;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = true)]
    public class SolutionController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SolutionController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Solution
        [AllowAnonymous]
        public ActionResult Index(bool? onlyMe, string question, string username, Languages? language, SolutionState? state, 
            int? page, string orderBy, bool? asc)
        {
            int currentUserId = User.Identity.GetIntUserId();
            var route = new RouteValueDictionary
            {
                {"onlyMe", onlyMe}, 
                {"question", question},
                {"username", username}, 
                {"language", language}, 
                {"state", state}, 
                {"orderBy", orderBy}, 
                {"asc", asc}
            };

            var query = _db.Solutions
                .OrderByDescending(x => x.SubmitTime)
                .Project().To<SolutionSummaryModel>();
            
            if (onlyMe != null && onlyMe.Value)
            {
                query = query.Where(x => x.CreateUserId == currentUserId);
            }
            if (!string.IsNullOrWhiteSpace(question))
            {
                query = query.Where(x => x.QuestionName == question);
            }
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(x => x.CreateUserName == username);
            }
            if (language != null)
            {
                query = query.Where(x => x.Language == language.Value);
            }
            if (state != null)
            {
                query = query.Where(x => x.State == state.Value);
            }

            var model = query.ToSortedPagedList(page, orderBy, asc);

            ViewBag.OnlyMe = onlyMe;
            ViewBag.Route = route;
            return View(model);
        }

        // GET: Solution/Source/5
        public async Task<ActionResult> Source(int id)
        {
            var model = await _db.Solutions
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    QuestionCreateUserId = x.Question.CreateUserId,
                    AuthorId = x.CreateUserId,
                    Source = x.Source
                })
                .FirstAsync();

            if (CheckAccess(model.AuthorId, model.QuestionCreateUserId))
            {
                return Content(model.Source);
            }

            return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
        }

        // GET: Solution/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var solution = await _db.Solutions
                .Project().To<SolutionDetailModel>()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (solution == null)
            {
                return RedirectToAction("Index").WithError(
                    string.Format("未找到id为{0}的解答。", id));
            }

            if (!CheckAccess(solution.CreateUserId, solution.QuestionCreateUserId))
            {
                return RedirectToAction("Index").WithInfo("只能查看自己的解答。");
            }
            return View(solution);
        }

        //
        // GET: Solution/Create/id
        public ActionResult Create(int? id)
        {
            var solutionCreateModel = new SolutionCreateModel {QuestionId = id??0};
            return View(solutionCreateModel);
        }

        // POST: Solution/Create/id
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SolutionCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var solution = Mapper.Map<Solution>(model);
                _db.Solutions.Add(solution);
                await _db.SaveChangesAsync();

                var judgeModel = await _db.Solutions
                    .Project().To<SolutionPushModel>()
                    .FirstOrDefaultAsync(x => x.Id == solution.Id);
                JudgeHub.Judge(judgeModel);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Solution/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // check access
            var acl = await _db.Solutions
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    AuthorId = x.CreateUserId, 
                    QuestionCreatorId = x.Question.CreateUserId
                })
                .FirstAsync();
            if (!CheckAccess(acl.AuthorId, acl.QuestionCreatorId))
            {
                return RedirectToAction("Index")
                    .WithWarning("只能删除自己提交的解答。");
            }

            // update
            await _db.Solutions.Where(x => x.Id == id).DeleteAsync();
            return RedirectToAction("Index").WithSuccess("解答删除成功。");
        }

        // POST: Solution/ReJudge/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ReJudge(int id)
        {
            // check access.
            var acl = await _db.Solutions
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    AuthorId = x.CreateUserId,
                    QuestionCreatorId = x.Question.CreateUserId
                })
                .FirstAsync();
            if (!CheckAccess(acl.AuthorId, acl.QuestionCreatorId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // act.
            await _db.Solutions
                .Where(x => x.Id == id)
                .UpdateAsync(s => new Solution {State = SolutionState.Queuing});

            var judgeModel = await _db.Solutions
                    .Project().To<SolutionPushModel>()
                    .FirstOrDefaultAsync(x => x.Id == id);
            SolutionHub.PushChange(judgeModel.Id, SolutionState.Queuing.GetDisplayName());
            JudgeHub.Judge(judgeModel);

            return new EmptyResult();
        }

        public bool CheckAccess(int authorId, int questionCreatorId)
        {
            IPrincipal user = User;
            IIdentity identity = user.Identity;
            int userId = identity.GetIntUserId();

            if (userId == authorId ||
                userId == questionCreatorId ||
                user.IsInRole(SystemRoles.QuestionAdmin))
            {
                return true;
            }
            return false;
        }
    }
}
