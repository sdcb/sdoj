using System.Diagnostics;
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

        public ActionResult Test()
        {
            var pid = Process.GetCurrentProcess().Id;
            return Content(pid.ToString());
        }

        public ActionResult Add()
        {
            ++Value;
            return Content(Value.ToString());
        }

        public static int Value { get; set; }
    }
}