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
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
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
            var model = _dbContext.Solutions.Project().To<SolutionSummaryModel>();
            return View(await model.ToListAsync());
        }

        // GET: Solution/Details/5
        [SdojAuthorize]
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

        //
        // GET: Solution/Create/id
        [SdojAuthorize]
        public ActionResult Create(int? id)
        {
            var solutionCreateModel = new SolutionCreateModel {QuestionId = id??0};
            return View(solutionCreateModel);
        }

        // POST: Solution/Create/id
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, SdojAuthorize, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "QuestionId,Language,Source")] SolutionCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var question = Mapper.Map<Solution>(model);
                
                _dbContext.Solutions.Add(question);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Solution/Delete/5
        [SdojAuthorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var solutions = _dbContext.Solutions.Project().To<SolutionDeleteModel>();
            var solution = await solutions.FirstOrDefaultAsync(x => x.Id == id);
            if (solution == null)
            {
                return HttpNotFound();
            }
            if (User.IsUserOrAdmin(solution.CreateUserId))
            {
                return RedirectToAction("Index")
                    .WithWarning("只能删除自己提交的解答。");
            }
            return View(solution);
        }

        // POST: Solution/Delete/5
        [HttpPost, ActionName("Delete"), SdojAuthorize()]
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
