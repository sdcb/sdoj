using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    [SdojAuthorize]
    public class ContestController : Controller
    {
        public ContestController(ContestManager manager, IPrincipal identity)
        {
            _manager = manager;
            _identity = identity;
        }

        public ActionResult Index()
        {
            int userId = _identity.Identity.GetUserId<int>();
            bool isManager = _identity.IsInRole(SystemRoles.ContestAuthor);
            return View(_manager.List(userId, isManager));
        }

        [SdojAuthorize(Roles = SystemRoles.ContestAuthor)]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Create(ContestCreateModel model)
        {
            int id = await _manager.Create(model);
            return RedirectToAction(nameof(Details), new { id = id });
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        private readonly ContestManager _manager;
        private readonly IPrincipal _identity;
    }
}
