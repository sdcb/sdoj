using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
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
using SdojWeb.Models.DbModels;
using Microsoft.AspNet.Identity;

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
        public ActionResult Index(int? id, bool? onlyMe, string question, string username, Languages? language, SolutionState? state, 
            int? page, string orderBy, bool? asc)
        {
            int currentUserId = User.Identity.GetUserId<int>(); 
            var route = new RouteValueDictionary
            {
                {"id", id }, 
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
            
            if (id != null)
            {
                query = query.Where(x => x.Id == id.Value);
            }
            if (onlyMe != null && onlyMe.Value)
            {
                query = query.Where(x => x.CreateUserId == currentUserId);
            }
            if (!string.IsNullOrWhiteSpace(question))
            {
                query = query.Where(x => x.QuestionName == question.Trim());
            }
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(x => x.CreateUserName == username.Trim());
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

            if (CheckAccess(model.AuthorId, model.QuestionCreateUserId) || User.IsInRole(SystemRoles.SolutionViewer))
            {
                return Content(model.Source, "text/plain");
            }

            return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
        }

        // POST: Solution/CompilerOutput/5
        [HttpPost]
        public async Task<ActionResult> CompilerOutput(int id)
        {
            var model = await _db.Solutions
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    QuestionCreateUserId = x.Question.CreateUserId,
                    AuthorId = x.CreateUserId,
                    CompilerOutput = x.CompilerOutput
                }).FirstAsync();

            if (CheckAccess(model.AuthorId, model.QuestionCreateUserId))
            {
                return Content(model.CompilerOutput, "text/plain");
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
        public ActionResult Create(int id, string name)
        {
            var solutionCreateModel = new SolutionCreateModel {QuestionId = id, Name = name};
            return View(solutionCreateModel);
        }

        // POST: Solution/Create/id
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
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

                Response.SetCookie(new HttpCookie(PreferredLanguage, model.Language.ToString()));

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
            if (!User.IsUserOrRole(acl.QuestionCreatorId, SystemRoles.QuestionAdmin))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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

            if (!User.IsUserOrRole(acl.QuestionCreatorId, SystemRoles.QuestionAdmin))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // act.
            await _db.Solutions
                .Where(x => x.Id == id)
                .UpdateAsync(s => new Solution {State = SolutionState.Queuing});
            await _db.SolutionLocks
                .Where(x => x.SolutionId == id)
                .DeleteAsync();

            var judgeModel = await _db.Solutions
                    .Project().To<SolutionPushModel>()
                    .FirstOrDefaultAsync(x => x.Id == id);
            SolutionHub.PushChange(judgeModel.Id, SolutionState.Queuing, 0, 0.0f);
            JudgeHub.Judge(judgeModel);

            return new EmptyResult();
        }

        public bool CheckAccess(int authorId, int questionCreatorId)
        {
            IPrincipal user = User;
            IIdentity identity = user.Identity;
            int userId = identity.GetUserId<int>();

            if (userId == authorId ||
                userId == questionCreatorId ||
                user.IsInRole(SystemRoles.QuestionAdmin))
            {
                return true;
            }
            return false;
        }

        public const string PreferredLanguage = "Preferred-Language";
    }
}
