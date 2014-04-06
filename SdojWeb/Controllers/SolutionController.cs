using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class SolutionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public SolutionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Solution
        public async Task<ActionResult> Index()
        {
            return View(await _dbContext.Solutions.ToListAsync());
        }

        // GET: Solution/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solution solution = await _dbContext.Solutions.FindAsync(id);
            if (solution == null)
            {
                return HttpNotFound();
            }
            return View(solution);
        }

        // GET: Solution/Create/id
        public ActionResult Create(int? id)
        {
            var solutionCreateModel = new SolutionCreateModel {QuestionId = id??0};
            return View(solutionCreateModel);
        }

        // POST: Solution/Create/id
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "QuestionId,Language,Source")] SolutionCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var question = Mapper.Map<Solution>(model);
                
                _dbContext.Solutions.Add(question);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw;
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Solution/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solution solution = await _dbContext.Solutions.FindAsync(id);
            if (solution == null)
            {
                return HttpNotFound();
            }
            return View(solution);
        }

        // POST: Solution/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include = "Id,ApplicationUserId,Language,Source,Status,SubmitTime")] Solution solution)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Entry(solution).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(solution);
        }

        // GET: Solution/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Solution solution = await _dbContext.Solutions.FindAsync(id);
            if (solution == null)
            {
                return HttpNotFound();
            }
            return View(solution);
        }

        // POST: Solution/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Solution solution = await _dbContext.Solutions.FindAsync(id);
            _dbContext.Solutions.Remove(solution);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
