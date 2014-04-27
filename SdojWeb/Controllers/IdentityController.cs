using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SdojWeb.Controllers
{
    public class IdentityController : Controller
    {
        // GET: Identity
        public ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        public ActionResult Users()
        {
            return View();
        }
    }
}