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
    [SdojAuthorize]
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
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Question");
            }
            var model = await _db.QuestionDatas
                .Project().To<QuestionDataEditModel>()
                .FirstOrDefaultAsync(x => x.Id == id.Value);

            if (model == null)
            {
                return RedirectToAction("Index", "Question")
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
    }
}
