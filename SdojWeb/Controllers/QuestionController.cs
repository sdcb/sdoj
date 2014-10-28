using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using EntityFramework.Extensions;
using SdojWeb.Infrastructure;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Infrastructure.Alerts;
using AutoMapper.QueryableExtensions;
using SdojWeb.Models.JudgePush;
using SdojWeb.SignalR;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = true)]
    public class QuestionController : Controller
    {
        public QuestionController(ApplicationDbContext db, QuestionManager manager)
        {
            _db = db;
            _manager = manager;
        }

        // GET: Questions
        [AllowAnonymous]
        public ActionResult Index(string name, string creator, 
            int? page, string orderBy, bool? asc)
        {
            var route = new RouteValueDictionary
            {
                {"name", name},
                {"creator",creator}
            };

            var query = _db.Questions
                .OrderByDescending(x => x.Id)
                .Project().To<QuestionSummaryViewModel>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(creator))
            {
                query = query.Where(x => x.Creator == creator.Trim());
            }

            var models = query.ToSortedPagedList(page, orderBy, asc);
            ViewBag.Route = route;
            return View(models);
        }

        // GET: Questions/5
        [AllowAnonymous]
        public async Task<ActionResult> Details(int id)
        {
            var question = await _db.Questions
                .Project().To<QuestionDetailModel>()
                .FirstAsync(x => x.Id == id);

            if (question == null)
            {
                return RedirectToAction("Index").WithError(
                    string.Format("没找到 id 为 {0} 的题目。", id));
            }

            return View(question);
        }

        // GET: Questions/Create
        [SdojAuthorize(Roles = SystemRoles.QuestionAdminOrCreator)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken, SdojAuthorize(Roles = SystemRoles.QuestionAdminOrCreator)]
        public async Task<ActionResult> Create(QuestionCreateModel createModel)
        {
            TransactionInRequest.EnsureTransaction();

            if (ModelState.IsValid)
            {
                if (await _manager.ExistName(createModel.Name))
                {
                    ModelState.AddModelError("Name", "已有同名的题目。");
                }
                else
                {
                    await _manager.Create(createModel);
                    return RedirectToAction("Index");
                }
            }

            return View(createModel);
        }

        // GET: Questions/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var question = await _db
                .Questions
                .Project().To<QuestionEditModel>()
                .FirstAsync(x => x.Id == id);

            if (!User.IsUserOrRole(question.CreateUserId, SystemRoles.QuestionAdmin))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
            }

            return View(question);
        }

        // POST: Questions/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(QuestionEditModel model)
        {
            if (ModelState.IsValid)
            {
                var secretModel = await _db.Questions
                    .Where(x => x.Id == model.Id)
                    .Project().To<QuestionNotMappedEditModel>()
                    .FirstOrDefaultAsync();

                if (!User.IsUserOrRole(secretModel.CreateUserId, SystemRoles.QuestionAdmin))
                {
                    return RedirectToAction("Details").WithWarning("仅题目创建者才能编辑题目。");
                }

                await _manager.Update(secretModel, model);
                return RedirectToAction("Details", new {id = model.Id});
            }
            return View(model);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var question = await _db.Questions.FindAsync(id);

            if (!User.IsUserOrRole(question.CreateUserId, SystemRoles.QuestionAdmin))
            {
                return RedirectToAction("Details").WithWarning("仅题目创建者才能删除题目。");
            }

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index").WithSuccess("题目删除成功。");
        }

        public async Task<ActionResult> CheckName(string name)
        {
            var exist = await _manager.ExistName(name);
            return Json(!exist, JsonRequestBehavior.AllowGet);
        }

        // GET: /Question/5/Data/
        [Route("Question/{id}/Data")]
        [AllowAnonymous]
        public async Task<ActionResult> Data(int id)
        {
            var questionDatas = await _db.QuestionDatas.Where(x => x.QuestionId == id)
                .Project().To<QuestionDataSummaryModel>()
                .OrderBy(x => x.Id)
                .ToArrayAsync();

            ViewBag.QuestionId = id;
            ViewBag.IsUserOwnsQuestion = await _manager.IsUserOwnsQuestion(id);
            ViewBag.QuestionName = questionDatas.First().QuestionName;

            return View(questionDatas);
        }
        
        // GET: /question/5/data/3
        [Route("Question/{questionId}/Data/Get")]
        public async Task<ActionResult> GetData(int questionId, int id)
        {
            var owns = await _manager.IsUserOwnsQuestion(questionId);
            if (!owns)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var data = await _db.QuestionDatas
                .Where(x => x.QuestionId == questionId && x.Id == id)
                .Select(x => new
                {
                    Input = x.Input,
                    Output = x.Output,
                    Time = x.TimeLimit,
                    Memory = x.MemoryLimitMb
                })
                .FirstAsync();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // POST: /question/5/data/save/3
        [Route("Question/{questionId}/Data/Save")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DataSave(int questionId, bool delete, int? id, string input, string output, int time, float memory)
        {
            TransactionInRequest.EnsureTransaction();

            var owns = await _manager.IsUserOwnsQuestion(questionId);
            if (!owns)
            {
                return NonOwnerReturn(questionId);
            }

            if (delete)
            {
                if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                await _manager.DeleteData(id.Value);
            }
            else
            {
                await _manager.SaveData(questionId, id, input, output, time, memory);
            }

            return RedirectToAction("Data", new {id = questionId});
        }

        private ActionResult NonOwnerReturn(int questionId)
        {
            return RedirectToAction("Data", new { id = questionId })
                .WithSuccess("只有创建者才能查看或操作测试数据。");
        }

        public async Task<ActionResult> ReJudge(int id)
        {
            TransactionInRequest.EnsureTransaction();

            var creator = await _db.Questions
                .Where(x => x.Id == id)
                .Select(x => x.CreateUserId)
                .FirstAsync();

            if (!User.IsUserOrRole(creator, SystemRoles.QuestionAdmin))
            {
                return RedirectToAction("Details", new {id = id})
                    .WithError("只有创建者才能重新评测该题目。");
            }

            await _db.Solutions
                .Where(x => x.QuestionId == id)
                .Select(x => x.Id)
                .ForEachAsync(x => SolutionHub.PushChange(x, SolutionState.Queuing, 0, 0.0f));

            await _db.Solutions
                .Where(x => x.QuestionId == id)
                .UpdateAsync(x => new Solution{State = SolutionState.Queuing});
            await _db.SolutionLocks
                .Where(x => x.Solution.QuestionId == id)
                .DeleteAsync();

            var solutions = _db.Solutions
                .Where(x => x.QuestionId == id)
                .Project().To<SolutionPushModel>();

            await solutions.ForEachAsync(JudgeHub.Judge);

            return RedirectToAction("Details", new {id = id})
                .WithSuccess("该题目重新评测已经开始。");
        }


        private readonly ApplicationDbContext _db;

        private readonly QuestionManager _manager;
    }
}
