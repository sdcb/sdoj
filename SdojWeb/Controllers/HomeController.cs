using System.Web.Mvc;

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

        [ActionName("Rect")]
        public ActionResult Rectangle()
        {
            return View();
        }
    }
}