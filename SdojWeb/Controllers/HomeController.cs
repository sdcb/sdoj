using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Models;
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