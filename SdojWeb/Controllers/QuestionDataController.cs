using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class QuestionDataController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuestionDataController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /QuestionData/ListForQuestion/id
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
    }
}
