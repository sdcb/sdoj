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
                orderBy = "Id";
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
                await _manager.Save(questionGroup);
                return this.JavascriptRedirectToAction("Index");
            }

            return View(questionGroup);
        }

        // GET: QuestionGroup/Edit/5
        public async Task<ActionResult> Edit(int? id)
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
            ViewBag.CreateUserId = new SelectList(_db.Users, "Id", "Email", questionGroup.CreateUserId);
            return View(questionGroup);
        }

        // POST: QuestionGroup/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,CreateTime,ModifyTime,CreateUserId")] QuestionGroup questionGroup)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(questionGroup).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreateUserId = new SelectList(_db.Users, "Id", "Email", questionGroup.CreateUserId);
            return View(questionGroup);
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
        public async Task<ActionResult> CheckName(string name)
        {
            name = name.Trim();
            var exist = await _manager.ExistName(name);
            return Json(!exist);
        }
    }
}
