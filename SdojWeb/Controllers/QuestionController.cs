using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using PagedList;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Infrastructure.Alerts;
using AutoMapper.QueryableExtensions;

namespace SdojWeb.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public QuestionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Questions
        public ActionResult Index(int? page, string orderBy, bool? asc)
        {
            var models = _dbContext.Questions.Project().To<QuestionSummaryViewModel>();
            var orderedPagedList = models.ToSortedPagedList(page, orderBy, asc);
            return View(orderedPagedList);
        }

        // GET: Questions/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await _dbContext.Questions.FindAsync(id);

            if (question == null)
            {
                return RedirectToAction("Index").WithError("没找到 id 为 {0} 的题目。", id);
            }

            return View(question);
        }

        // GET: Questions/Create
        [SdojAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, SdojAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,SampleInput,SampleOutput,MemoryLimitMB,TimeLimit")] QuestionCreateModel createModel)
        {
            if (ModelState.IsValid)
            {
                var question = Mapper.Map<Question>(createModel);
                _dbContext.Questions.Add(question);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(createModel).WithError("ModelState构造失败");
        }

        // GET: Questions/Edit/5
        [SdojAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("没找到该题目。");
            }

            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
            }

            return View(question);
        }

        // POST: Questions/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, SdojAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,SampleInput,SampleOutput,MemoryLimitMB,TimeLimit")] Question question)
        {
            if (ModelState.IsValid)
            {
                var owner = _dbContext.Questions.Where(x => x.Id == question.Id).Select(x => x.CreateUserId).FirstOrDefault();
                if (!User.IsUserOrAdmin(owner))
                {
                    return RedirectToAction("Index").WithWarning("仅题目创建者才能编辑题目。");
                }
                question.CreateTime = DateTime.Now;
                question.CreateUserId = User.Identity.GetIntUserId();
                _dbContext.Entry(question).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        [SdojAuthorize]
        public async Task<ActionResult> Delete(int id)
        {
            var question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("未找到该题目。");
            }
            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能删除题目。");
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete"), SdojAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return RedirectToAction("Index").WithError("未找到该题目。");
            }
            if (!User.IsUserOrAdmin(question.CreateUserId))
            {
                return RedirectToAction("Index").WithWarning("仅题目创建者才能删除题目。");
            }

            _dbContext.Questions.Remove(question);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
