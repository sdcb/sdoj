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
using SdojWeb.Models.DbModels;
using System.Text;
using System;

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
        public ActionResult Index(string name, string creator, QuestionTypes? type, bool? me,
            int? page, string orderBy, bool? asc)
        {
            if (orderBy == null)
            {
                orderBy = "Id";
                asc = false;
            }

            var route = new RouteValueDictionary
            {
                {"name", name},
                {"creator",creator},
                {"type",type},
                {"me",me}
            };

            var query = _manager.List(name, creator, type, me);
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
            return View(new QuestionCreateModel());
        }

        // POST: Questions/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken, SdojAuthorize(Roles = SystemRoles.QuestionAdminOrCreator)]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(QuestionCreateModel createModel)
        {
            if (ModelState.IsValid)
            {
                using (var tran = TransactionInRequest.BeginTransaction())
                {
                    if (!await _manager.CheckName(createModel.Name))
                    {
                        ModelState.AddModelError("Name", "已有同名的题目。");
                    }
                    else
                    {
                        await _manager.Create(createModel);
                        return RedirectToAction("Index");
                    }
                    tran.Complete();
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
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
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
                return RedirectToAction("Details", new { id = model.Id });
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

        [HttpPost]
        public async Task<ActionResult> CheckName(string name, int? id)
        {
            name = name.Trim();
            var valid = await _manager.CheckName(name, id);
            return Json(valid);
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
            ViewBag.QuestionName = (await _db.Questions.FindAsync(id)).Name;

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

        [Route("Question/{questionId}/Data/{id}/Input")]
        public async Task<ActionResult> DownloadInputData(int questionId, int id)
        {
            var owns = await _manager.IsUserOwnsQuestion(questionId);
            if (!owns)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var input = await _db.QuestionDatas
                .Where(x => x.QuestionId == questionId && x.Id == id)
                .Select(x => x.Input)
                .FirstAsync();

            var filename = string.Format("{0}-{1}-input.txt", questionId, id);
            return File(Encoding.UTF8.GetBytes(input), "text/plain", filename);
        }

        [Route("Question/{questionId}/Data/{id}/Output")]
        public async Task<ActionResult> DownloadOutputData(int questionId, int id)
        {
            var owns = await _manager.IsUserOwnsQuestion(questionId);
            if (!owns)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var output = await _db.QuestionDatas
                .Where(x => x.QuestionId == questionId && x.Id == id)
                .Select(x => x.Output)
                .FirstAsync();

            var filename = string.Format("{0}-{1}-output.txt", questionId, id);
            return File(Encoding.UTF8.GetBytes(output), "text/plain", filename);
        }

        // POST: /question/5/data/save/3
        [Route("Question/{questionId}/Data/Save")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DataSave(int questionId, int? id, string input, string output, int time, float memory, bool isSample)
        {
            // 此处Save表示可以添加或者更新，但不能删除（删除使用DataDelete）。
            // id == null: 添加；
            // id != null: 更新。
            using (var tran = TransactionInRequest.BeginTransaction())
            {
                var owns = await _manager.IsUserOwnsQuestion(questionId);
                if (!owns)
                {
                    return NonOwnerReturn(questionId);
                }

                await _manager.SaveData(questionId, id, input, output, time, memory, isSample);
                tran.Complete();
            }

            return RedirectToAction("Data", new { id = questionId });
        }

        // POST: /question/5/data/save/3
        [Route("Question/{questionId}/Data/Delete")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DataDelete(int questionId, int id)
        {
            using (var tran = TransactionInRequest.BeginTransaction())
            {
                var owns = await _manager.IsUserOwnsQuestion(questionId);
                if (!owns)
                {
                    return NonOwnerReturn(questionId);
                }

                await _manager.DeleteData(id);
                tran.Complete();
            }

            return RedirectToAction("Data", new { id = questionId });
        }

        private ActionResult NonOwnerReturn(int questionId)
        {
            return RedirectToAction("Data", new { id = questionId })
                .WithSuccess("只有创建者才能查看或操作测试数据。");
        }

        public async Task<ActionResult> ReJudge(int id)
        {
            using (var tran = TransactionInRequest.BeginTransaction())
            {
                var creator = await _db.Questions
                .Where(x => x.Id == id)
                .Select(x => x.CreateUserId)
                .FirstAsync();

                if (!User.IsUserOrRole(creator, SystemRoles.QuestionAdmin))
                {
                    return RedirectToAction("Details", new { id = id })
                        .WithError("只有创建者才能重新评测该题目。");
                }

                await _db.Solutions
                    .Where(x => x.QuestionId == id)
                    .Select(x => x.Id)
                    .ForEachAsync(x => SolutionHub.PushChange(x, SolutionState.Queuing, 0, 0.0f));

                await _db.Solutions
                    .Where(x => x.QuestionId == id)
                    .UpdateAsync(x => new Solution { State = SolutionState.Queuing });
                await _db.SolutionLocks
                    .Where(x => x.Solution.QuestionId == id)
                    .DeleteAsync();

                var solutions = _db.Solutions
                    .Where(x => x.QuestionId == id)
                    .Project().To<SolutionPushModel>();

                await solutions.ForEachAsync(JudgeHub.Judge);

                tran.Complete();
            }

            return RedirectToAction("Details", new { id = id })
                .WithSuccess("该题目重新评测已经开始。");
        }


        private readonly ApplicationDbContext _db;

        private readonly QuestionManager _manager;
    }
}
