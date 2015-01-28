using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SdojWeb.Models;
using SdojWeb.Models.DbModels;
using AutoMapper.QueryableExtensions;
using System.Web.Routing;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Manager;
using System.Linq;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using AutoMapper;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure;

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
            var models = query.Project().To<QuestionGroupListModel>();
            if (orderBy == null)
            {
                orderBy = "ModifyTime";
                asc = false;
            }
            var paged = models.ToSortedPagedList(page, orderBy, asc);

            return View(paged);
        }

        // GET: QuestionGroup/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var model = await _db.QuestionGroups
                .Project().To<QuestionGroupDetailModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        // GET: QuestionGroup/Create
        [SdojAuthorize(EmailConfirmed = true)]
        public ActionResult Create()
        {
            SetQuestionRoutes();

            var model = new QuestionGroupEditModel();
            ViewData["EditMode"] = false;
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

            var questions = _questionManager.BasicList(name, creator)
                .ToSortedPagedList(page, orderBy, asc);

            return PartialView("_Question", questions);
        }

        // POST: QuestionGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SdojAuthorize(EmailConfirmed = true)]
        public async Task<ActionResult> Create(QuestionGroupEditModel questionGroup)
        {
            var x = ModelState;
            if (ModelState.IsValid)
            {
                await _manager.Create(questionGroup);
                return this.JavascriptRedirectToAction("Index");
            }

            ViewData["EditMode"] = false;
            return View(questionGroup);
        }

        // GET: QuestionGroup/Edit/5
        [SdojAuthorize(EmailConfirmed = true)]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var questionGroup = await _db.QuestionGroups
                .Project().To<QuestionGroupEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id);

            SetQuestionRoutes();

            if (questionGroup == null)
            {
                return HttpNotFound();
            }

            ViewData["EditMode"] = true;
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
                if (owner != User.Identity.GetUserId<int>())
                {
                    return RedirectToAction("Index").WithError("只能修改自己的题目组。");
                }

                await _manager.Save(questionGroup); 

                return RedirectToAction("Index");
            }
            ViewData["EditMode"] = true;
            return View("Create", questionGroup);
        }

        // GET: QuestionGroup/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionGroup questionGroup = await _db.QuestionGroups.FindAsync(id);
            if (questionGroup == null)
            {
                return HttpNotFound();
            }
            return View(questionGroup);
        }

        // POST: QuestionGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            QuestionGroup questionGroup = await _db.QuestionGroups.FindAsync(id);
            _db.QuestionGroups.Remove(questionGroup);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> CheckName(string name, int id)
        {
            bool valid;
            if (id != 0) // Only Check Name in Create model.
            {
                valid = true;
            }
            else
            {
                name = name.Trim();
                var exist = await _manager.ExistName(name);
                valid = !exist;
            }
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

            var questions = _questionManager.BasicList(null, null).ToSortedPagedList(1, "Id", false);
            ViewData["Question"] = questions;
        }
    }
}
