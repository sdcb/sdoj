using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;

namespace SdojWeb.Controllers
{
    [SdojAuthorize]
    [RoutePrefix("contest")]
    public class ContestController : Controller
    {
        public ContestController(
            ContestManager manager, 
            IPrincipal identity, 
            ApplicationDbContext db)
        {
            _manager = manager;
            _identity = identity;
            _db = db;
        }

        public ActionResult Index()
        {
            bool isManager = _identity.IsInRole(SystemRoles.ContestAdmin);
            IQueryable<ContestListModel> data = _manager.List(GetCurrentUserId(), isManager);
            return View(data);
        }

        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new ContestCreateModel());
        }

        [ValidateAntiForgeryToken, HttpPost, ActionName(nameof(Create))]
        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        public async Task<ActionResult> CreateConfirmed(ContestCreateModel model)
        {
            await model.Validate(_db, ModelState);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int id = await _manager.Create(model, GetCurrentUserId());
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Route("details/{contestId}")]
        [Route("details/{contestId}/question/{rank}")]
        public async Task<ActionResult> Details(int contestId, int rank = 1)
        {
            if (!await _manager.CheckAccess(contestId, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return RedirectToAction("Index").WithWarning("此考试不存在或你无权限访问。");
            }
            ContestDetailsModel details = await _manager.Get(contestId);
            details.CurrentQuestion = await _manager.GetQuestion(contestId, rank);
            details.Rank = rank;
            return View(details);
        }

        [Route("details/{contestId}/question-{questionId}/solutions")]
        public async Task<ActionResult> QuestionSolutions(int contestId, int questionId)
        {
            if (!await _manager.CheckAccess(contestId, questionId, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return new HttpUnauthorizedResult();
            }

            return Json(await _manager.GetQuestionSolutions(questionId));
        }

        private int GetCurrentUserId()
        {
            return _identity.Identity.GetUserId<int>();
        }

        private readonly ContestManager _manager;
        private readonly IPrincipal _identity;
        private readonly ApplicationDbContext _db;
    }
}
