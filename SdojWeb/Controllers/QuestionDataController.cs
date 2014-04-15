using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(Roles="admin")]
    public class QuestionDataController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuestionDataController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /QuestionData/ListForQuestion/5
        public async Task<ActionResult> ListForQuestion(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Question").WithInfo("没找到给定的题目。");
            }
            var questionDatas = await _db.QuestionDatas.Where(x => x.QuestionId == id)
                .Project().To<QuestionDataSummaryModel>()
                .ToArrayAsync();

            ViewBag.QuestionId = id;
            if (questionDatas.Length == 0)
            {
                return View(questionDatas).WithInfo("该题目目前没有任何测试数据。");
            }

            return View(questionDatas);
        }

        // GET: /QuestionData/CreateForQuestion/5
        public ActionResult CreateForQuestion(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Question");
            }
            var model = new QuestionDataEditModel {QuestionId = id.Value};
            return View(model);
        }

        // POST: /QuestionData/CreateForQuestion
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateForQuestion(QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                var questionData = Mapper.Map<QuestionData>(model);

                _db.QuestionDatas.Add(questionData);
                await _db.SaveChangesAsync();
                return RedirectToAction("ListForQuestion", new {id = model.QuestionId})
                    .WithInfo("已成功创建该测试数据。");
            }
            
            return View(model);
        }

        // GET: /QuestionData/Edit/5
        public async Task<ActionResult> Edit(int? id, int questionId)
        {
            if (id == null)
            {
                return RedirectToAction("ListForQuestion", new {id = questionId})
                    .WithError("id不能为空。");
            }
            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (model == null)
            {
                return RedirectToAction("ListForQuestion", new {id=questionId})
                    .WithInfo("未找到id为{0}的测试数据。", id);
            }

            return View(model);
        }

        // POST: /QuestionData/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="Id,QuestionId,Input,Output")] QuestionDataEditModel model)
        {
            if (ModelState.IsValid)
            {
                var questionData = Mapper.Map<QuestionData>(model);
                _db.Entry(questionData).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("ListForQuestion", new {id = model.QuestionId})
                    .WithInfo("测试数据保存成功。");
            }
            return View(model);
        }

        // GET: /QuestionData/Delete/5
        public async Task<ActionResult> Delete(int? id, int? questionId)
        {
            if (id == null)
            {
                return RedirectToAction("ListForQuestion", new {id = questionId});
            }

            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id.Value);
            if (model == null)
            {
                return RedirectToAction("ListForQuestion", new { id = questionId })
                    .WithError("没找到Id为{0}的测试数据。", id);
            }

            return View(model);
        }

        // POST: /QuestionData/DeleteConfirmed/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, int questionId)
        {
            var model = await _db.QuestionDatas.FindAsync(id);
            if (model == null)
            {
                return RedirectToAction("ListForQuestion", new { id = questionId })
                    .WithError("没找到Id为{0}的测试数据。", id);
            }

            _db.Entry(model).State = EntityState.Deleted;
            var affectedRows = await _db.SaveChangesAsync();
            // assert affectedRows = 1.

            return RedirectToAction("ListForQuestion", new { id = questionId })
                .WithSuccess("Id为{0}的测试数据删除成功。", id);
        }
    }
}
