using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EntityFramework.Extensions;
using PagedList;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Models.JudgePush;
using SdojWeb.SignalR;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = true)]
    public class SolutionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public SolutionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Solution
        [AllowAnonymous]
        public ActionResult Index(int? page, bool? onlyMe)
        {
            page = page ?? 1;
            onlyMe = onlyMe ?? false;
            var model = _dbContext.Solutions
                .OrderByDescending(x => x.SubmitTime)
                .Project().To<SolutionSummaryModel>();
            var currentUserId = User.Identity.GetIntUserId();
            if (onlyMe.Value)
            {
                model = model.Where(x => x.CreateUserId == currentUserId);
            }
            ViewBag.OnlyMe = onlyMe;
            return View(model.ToPagedList(page.Value, AppSettings.DefaultPageSize));
        }

        // GET: Solution/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var solution = await _dbContext.Solutions
                .Project().To<SolutionDetailModel>()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (solution == null)
            {
                return RedirectToAction("Index").WithError(
                    string.Format("未找到id为{0}的解答。", id));
            }
            if (!User.IsUserOrAdmin(solution.CreateUserId))
            {
                return RedirectToAction("Index").WithInfo("只能查看自己的解答。");
            }
            return View(solution);
        }

        //
        // GET: Solution/Create/id
        public ActionResult Create(int? id)
        {
            var solutionCreateModel = new SolutionCreateModel {QuestionId = id??0};
            return View(solutionCreateModel);
        }

        // POST: Solution/Create/id
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "QuestionId,Language,Source")] SolutionCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var solution = Mapper.Map<Solution>(model);
                _dbContext.Solutions.Add(solution);
                await _dbContext.SaveChangesAsync();

                var judgeModel = await _dbContext.Solutions
                    .Project().To<SolutionPushModel>()
                    .FirstOrDefaultAsync(x => x.Id == solution.Id);
                JudgeHub.Judge(judgeModel);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Solution/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Solution solution = await _dbContext.Solutions.FindAsync(id);
            if (!User.IsUserOrAdmin(solution.CreateUserId))
            {
                return RedirectToAction("Index")
                    .WithWarning("只能删除自己提交的解答。");
            }

            _dbContext.Solutions.Remove(solution);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index").WithSuccess("解答删除成功。");
        }

        // POST: Solution/ReJudge/5
        [HttpPost, ValidateAntiForgeryToken, SdojAuthorize(Roles = SystemRoles.Admin)]
        public async Task<ActionResult> ReJudge(int id)
        {
            await _dbContext.Solutions
                .Where(x => x.Id == id)
                .UpdateAsync(s => new Solution {Status = SolutionStatus.Queuing});

            var judgeModel = await _dbContext.Solutions
                    .Project().To<SolutionPushModel>()
                    .FirstOrDefaultAsync(x => x.Id == id);
            SolutionHub.PushChange(judgeModel.Id, SolutionStatus.Queuing.GetDisplayName());
            JudgeHub.Judge(judgeModel);

            return RedirectToAction("Details", new {id = id})
                .WithSuccess("重新评测成功。");
        }
    }
}
