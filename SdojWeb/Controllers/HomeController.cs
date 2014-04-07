using System.Web.Mvc;
using SdojWeb.Infrastructure.Filters;

namespace SdojWeb.Controllers
{
    public class HomeController : Controller
    {
        [ShowUserIsConfirmedFilter]
        public ActionResult Index()
        {
            return View();
        }
    }
}