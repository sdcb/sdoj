using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SdojWeb.Models;
using SdojWeb.Models.DbModels;
using AutoMapper.QueryableExtensions;
using System.Web.Routing;
using System.Linq;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Extensions;

namespace SdojWeb.Controllers
{
    [RoutePrefix("Group")]
    public class QuestionGroupController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuestionGroupController(ApplicationDbContext db)
        {
            _db = db;
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
                { "author", author }
            };
            ViewData["Route"] = route;

            var query = _db.QuestionGroups.AsQueryable();

            if (id != null)
            {
                query = query.Where(x => x.Id == id);
            }
            if (onlyMe != null)
            {
                var userId = User.Identity.GetUserId<int>();
                query = query.Where(x => x.CreateUserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                query = query.Where(x => x.Name.StartsWith(name));
            }
            if (!string.IsNullOrWhiteSpace(author))
            {
                author = author.Trim();
                query = query.Where(x => x.CreateUser.UserName == author);
            }

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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuestionGroup questionGroup = await _db.QuestionGroups.FindAsync(id);
            if (questionGroup == null)
            {
                return HttpNotFound();
            }
            return View(questionGroup);
        }

        // GET: QuestionGroup/Create
        public ActionResult Create()
        {
            ViewBag.CreateUserId = new SelectList(_db.Users, "Id", "Email");
            return View();
        }

        // POST: QuestionGroup/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,CreateTime,ModifyTime,CreateUserId")] QuestionGroup questionGroup)
        {
            if (ModelState.IsValid)
            {
                _db.QuestionGroups.Add(questionGroup);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreateUserId = new SelectList(_db.Users, "Id", "Email", questionGroup.CreateUserId);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
