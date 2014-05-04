using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using PagedList;
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
        [SdojAuthorize]
        public async Task<ActionResult> Details(int id)
        {
            var solution = await _dbContext.Solutions
                .Project().To<SolutionDetailModel>()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (solution == null)
            {
                return RedirectToAction("Index").WithError("未找到id为{0}的解答。", id);
            }
            if (!User.IsUserOrAdmin(solution.CreateUserId))
            {
                return RedirectToAction("Index").WithInfo("只能查看自己的解答。");
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
                var solution = Mapper.Map<Solution>(model);
                _dbContext.Solutions.Add(solution);
                await _dbContext.SaveChangesAsync();

                var signalr = GlobalHost.ConnectionManager.GetConnectionContext<JudgeConnection>();
                //await signalr.Groups.Send(User.Identity.GetUserName(), solution);
                await signalr.Connection.Broadcast(solution);

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
            if (!User.IsUserOrAdmin(solution.CreateUserId))
            {
                return RedirectToAction("Index")
                    .WithWarning("只能删除自己提交的解答。");
            }
            return View(solution);
        }

        // POST: Solution/Delete/5
        [HttpPost, ActionName("Delete"), SdojAuthorize]
        [ValidateAntiForgeryToken]
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
            return RedirectToAction("Index");
        }
    }
}
