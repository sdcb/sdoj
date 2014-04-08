using Microsoft.AspNet.Identity.Owin;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Owin;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Infrastructure.Filters
{
    public class ShowUserIsConfirmedFilter : ActionFilterAttribute
    {
        public ICurrentUser CurrentUser { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var context = filterContext.HttpContext;
            var logined = context.User.Identity.IsAuthenticated;
            if (!logined) return;

            var confirmed = CurrentUser.User.EmailConfirmed;
            filterContext.Controller.TempData["Confirmed"] = confirmed;

            base.OnActionExecuted(filterContext);
        }
    }
}