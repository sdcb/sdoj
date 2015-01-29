using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using SdojWeb.Models;
using SdojWeb.Models.DbModels;
using AutoMapper.QueryableExtensions;
using System.Web.Routing;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Manager;
using System.Linq;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Infrastructure.Alerts;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Controllers
{
    public class QuestionGroupController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly QuestionGroupManager _manager;

        private readonly QuestionManager _questionManager;

        public QuestionGroupController(ApplicationDbContext db, QuestionGroupManager manager, QuestionManager questionManager)
        {
            _db = db;
            _manager = manager;
            _questionManager = questionManager;
        }

        // GET: QuestionGroup
        public ActionResult Index(int? id, bool? onlyMe, string name, string author, 
            int? page, string orderBy, bool? asc)
        {
            var route = new RouteValueDictionary()
            {
                { "id", id },
                { "onlyMe", onlyMe},
                { "name", name },
                { "author", author },
                { "page", page },
                { "orderBy", orderBy },
                { "asc", asc }
            };
            ViewData["Route"] = route;

            var query = _manager.List(id, onlyMe, name, author);
            var models = query.Project().To<QuestionGroupListModel>(new { currentUserId = User.Identity.GetUserId<int>() });
            if (orderBy == null)
            {
                orderBy = "ModifyTime";
                asc = false;
            }
            var paged = models.ToSortedPagedList(page, orderBy, asc);

            return View(paged);
        }

        // GET: QuestionGroup/Details/5
        public async Task<ActionResult> Details(int id, int? page, bool? passed)
        {
            var model = await _db.QuestionGroups
                .Project().To<QuestionGroupDetailModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            var query = _db.QuestionGroupItems
                .Where(x => x.QuestionGroupId == id)
                .OrderBy(x => x.Order)
                .Project().To<QuestionGroupDetailItemModel>(new { currentUserId = User.Identity.GetUserId<int>() });

            if (passed != null)
            {
                query = query.Where(x => x.Question.Complished == passed);
            }
            var items = query.ToSortedPagedList(page, null, null);
            ViewData["QuestionItems"] = items;

            return View(model);
        }

        // GET: QuestionGroup/Create
        [SdojAuthorize(EmailConfirmed = true, Roles = SystemRoles.QuestionGroupAdminOrCreator)]
        public ActionResult Create()
        {
            SetQuestionRoutes();

            var model = new QuestionGroupEditModel();
            return View(model);
        }

        // POST: QuestionGroup/Question
        public ActionResult Question(string name, string creator, 
            int? page, string orderBy, bool? asc)
        {
            var route = new RouteValueDictionary
            {
                { "name", name },
                { "creator", creator },
                { "page", page },
                { "orderBy", orderBy },
                { "asc", asc }
            };
            ViewData["QuestionRoute"] = route;

            if (orderBy == null)
            {
                orderBy = "Id";
                asc = false;
            }

            var questions = _questionManager.List(name, creator)
                .ToSortedPagedList(page, orderBy, asc);

            return PartialView("_Question", questions);
        }

        // POST: QuestionGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SdojAuthorize(EmailConfirmed = true, Roles = SystemRoles.QuestionGroupAdminOrCreator)]
        public async Task<ActionResult> Create(QuestionGroupEditModel questionGroup)
        {
            var x = ModelState;
            if (ModelState.IsValid)
            {
                await _manager.Create(questionGroup);
                return this.JavascriptRedirectToAction("Index");
            }

            return View(questionGroup);
        }

        // GET: QuestionGroup/Edit/5
        [SdojAuthorize(EmailConfirmed = true)]
        public async Task<ActionResult> Edit(int id)
        {
            var owner = await _db.QuestionGroups
                .Where(x => x.Id == id)
                .Select(x => x.CreateUserId)
                .FirstOrDefaultAsync();

            if (!User.IsUserOrRole(owner, SystemRoles.QuestionGroupAdmin))
            {
                return RedirectToAction("Index").WithError("只有作者才能修改此题目组。");
            }

            var questionGroup = await _db.QuestionGroups
                .Project().To<QuestionGroupEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            SetQuestionRoutes();

            return View("Create", questionGroup);
        }

        // POST: QuestionGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SdojAuthorize(EmailConfirmed = true)]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(QuestionGroupEditModel questionGroup)
        {
            if (ModelState.IsValid)
            {
                var owner = await _db.QuestionGroups
                    .Where(x => x.Id == questionGroup.Id)
                    .Select(x => x.CreateUserId)
                    .FirstOrDefaultAsync();

                if (!User.IsUserOrRole(owner, SystemRoles.QuestionGroupAdmin))
                {
                    return this.JavascriptRedirectToAction("Index").WithJavascriptAlert("只有作者才能修改此题目组。");
                }

                await _manager.Save(questionGroup);
                return Json(true);
            }
            return View("Create", questionGroup);
        }

        // POST: QuestionGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            QuestionGroup questionGroup = await _db.QuestionGroups.FindAsync(id);

            if (!User.IsUserOrRole(questionGroup.CreateUserId, SystemRoles.QuestionGroupAdmin))
            {
                return RedirectToAction("Index").WithError("只有作者才能修改此题目组。");
            }

            _db.QuestionGroups.Remove(questionGroup);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index").WithSuccess("题目组 " + questionGroup.Name + "删除成功！");
        }

        [HttpPost]
        public async Task<ActionResult> CheckName(string name, int id)
        {
            name = name.Trim();

            var valid = await _manager.CheckName(name, id);
            return Json(valid);
        }

        private void SetQuestionRoutes()
        {
            var route = new RouteValueDictionary
            {
                { "name", "" },
                { "creator", "" },
                { "page", 1 },
                { "orderBy", "Id" },
                { "asc", false }
            };
            ViewData["QuestionRoute"] = route;

            var questions = _questionManager.List(null, null).ToSortedPagedList(1, "Id", false);
            ViewData["Question"] = questions;
        }
    }
}
