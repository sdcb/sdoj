using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Infrastructure.Alerts;
using AutoMapper.QueryableExtensions;

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
                query = query.Where(x => x.Name.StartsWith(name));
            }
            if (!string.IsNullOrWhiteSpace(creator))
            {
                query = query.Where(x => x.Creator == creator);
            }

            var models = query.ToSortedPagedList(page, orderBy, asc);
            ViewBag.Route = route;
            return View(models);
        }

        // GET: Questions/Details/5
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
                    return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
                }

                await _manager.Update(secretModel, model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var question = await _db.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("未找到该题目。");
            }
            if (!User.IsUserOrRole(question.CreateUserId, SystemRoles.QuestionAdmin))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能删除题目。");
            }

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index").WithSuccess("删除成功。");
        }

        public async Task<ActionResult> CheckName(string name)
        {
            var exist = await _manager.ExistName(name);
            return Json(!exist, JsonRequestBehavior.AllowGet);
        }
        
        // GET: /Question/5/Data/
        [Route("Question/{id}/Data")]
        public async Task<ActionResult> Data(int id)
        {
            var questionDatas = await _db.QuestionDatas.Where(x => x.QuestionId == id)
                .Project().To<QuestionDataSummaryModel>()
                .ToArrayAsync();

            ViewBag.QuestionId = id;
            ViewBag.IsUserOwnsQuestion = await _manager.IsUserOwnsQuestion(id);
            return View(questionDatas);
        }

        // GET: /Question/5/Data/Create
        [Route("Question/{id}/Data/Create")]
        public async Task<ActionResult> DataCreate(int id)
        {
            if (await _manager.IsUserOwnsQuestion(id))
            {
                var model = new QuestionDataEditModel
                {
                    QuestionId = id,
                    QuestionName = await _manager.GetName(id)
                };
                return View(model);
            }
            return NonOwnerReturn(id);
        }

        // POST: /Question/5/Data/Create
        [Route("Question/{id}/Data/Create")]
        [HttpPost, ValidateAntiForgeryToken, ActionName("DataCreate")]
        public async Task<ActionResult> DataCreateConfirmed(QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _manager.IsUserOwnsQuestion(model.QuestionId))
                {
                    var questionData = Mapper.Map<QuestionData>(model);

                    _db.QuestionDatas.Add(questionData);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Data", new { id = model.QuestionId })
                        .WithInfo("已成功创建该测试数据。");
                }

                return NonOwnerReturn(model.QuestionId);
            }

            return View(model);
        }

        // GET: /Question/5/Data/Edit
        [Route("Question/{questionId}/Data/{id}/Edit")]
        public async Task<ActionResult> DataEdit(int id, int questionId)
        {
            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (User.IsUserOrRole(model.CreateUserId, SystemRoles.QuestionAdmin))
            {
                return View(model);
            }

            return NonOwnerReturn(model.QuestionId);
        }

        // POST: /Question/5/Data/Edit
        [Route("Question/{questionId}/Data/{id}/Edit")]
        [HttpPost, ValidateAntiForgeryToken, ActionName("DataEdit")]
        public async Task<ActionResult> DataEditConfirmed(QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = await _db.QuestionDatas
                    .Where(x => x.Id == model.Id)
                    .Select(x => x.Question.CreateUserId)
                    .FirstAsync();

                if (User.IsUserOrRole(userId, SystemRoles.QuestionAdmin))
                {
                    var questionData = Mapper.Map<QuestionData>(model);
                    _db.Entry(questionData).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Data", new { id = model.QuestionId })
                        .WithInfo("测试数据保存成功。");
                }

                return NonOwnerReturn(model.QuestionId);
            }
            return View(model);
        }

        // POST: /Question/5/Data/5/Delete
        [Route("Question/{questionId}/Data/Delete")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DataDelete(int id, int questionId)
        {
            if (!await _manager.IsUserOwnsQuestion(questionId))
            {
                return NonOwnerReturn(questionId);
            }

            var model = await _db.QuestionDatas.FindAsync(id);
            _db.Entry(model).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("Data", new { id = questionId })
                .WithSuccess("测试数据删除成功。");
        }

        private ActionResult NonOwnerReturn(int questionId)
        {
            return RedirectToAction("Data", new { id = questionId })
                .WithSuccess("只有创建者才能查看或操作测试数据。");
        }


        private readonly ApplicationDbContext _db;

        private readonly QuestionManager _manager;
    }
}
