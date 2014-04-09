using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Infrastructure.Alerts;
using AutoMapper.QueryableExtensions;
using SdojWeb.Infrastructure;
using AutoMapper;

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
        public async Task<ActionResult> Index()
        {
            var models = await _dbContext.Questions.Project().To<QuestionSummaryViewModel>().ToListAsync();
            return View(models);
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
        [SdojAuthorize(Roles = "admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, SdojAuthorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,Description,SampleInput,SampleOutput,MemoryLimitMB,TimeLimit")] Question question)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Questions.Add(question);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(question).WithError("ModelState构造失败");
        }

        // GET: Questions/Edit/5
        [SdojAuthorize(Roles = "admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, SdojAuthorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,SampleInput,SampleOutput,MemoryLimitMB,TimeLimit")] Question question)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Entry(question).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        [SdojAuthorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await _dbContext.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete"), SdojAuthorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Question question = await _dbContext.Questions.FindAsync(id);
            _dbContext.Questions.Remove(question);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
