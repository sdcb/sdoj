using System.Web.Mvc;
using SdojWeb.Infrastructure.Filters;

namespace SdojWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }
    }
}