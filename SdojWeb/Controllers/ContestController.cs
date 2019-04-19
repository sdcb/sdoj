using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;

namespace SdojWeb.Controllers
{
    [SdojAuthorize]
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
            bool isManager = _identity.IsInRole(SystemRoles.ContestAuthor);
            IQueryable<ContestListModel> data = _manager.List(GetCurrentUserId(), isManager);
            return View(data);
        }

        [SdojAuthorize(Roles = SystemRoles.ContestAuthor)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new ContestCreateModel());
        }

        [ValidateAntiForgeryToken, HttpPost, ActionName(nameof(Create))]
        [SdojAuthorize(Roles = SystemRoles.ContestAuthor)]
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

        public ActionResult Details(int id)
        {
            return View();
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
