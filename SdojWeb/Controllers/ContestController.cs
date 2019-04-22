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

        [Route("details/{id}")]
        [Route("details/{id}/question/{rank}")]
        public async Task<ActionResult> Details(int id, int rank = 1)
        {
            if (!await _manager.CheckAccess(id, User.IsInRole(SystemRoles.ContestAdmin), GetCurrentUserId()))
            {
                return RedirectToAction("Index").WithWarning("此考试不存在或你无权限访问。");
            }
            ContestDetailsModel details = await _manager.Get(id, rank);
            ViewBag.Rank = rank;
            return View(details);
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
