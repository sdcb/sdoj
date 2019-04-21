using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Alerts;

namespace SdojWeb.Infrastructure.Identity
{
    public class SdojAuthorizeAttribute : AuthorizeAttribute
    {
        public bool EmailConfirmed { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            if (EmailConfirmed)
            {
                return httpContext.User.EmailConfirmed();
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                return;
            }

            var msgs = new List<string>();
            if (EmailConfirmed)
                msgs.Add("经过邮件验证");
            if (!string.IsNullOrWhiteSpace(Users))
                msgs.Add("用户名为<" + Users + ">");
            if (!string.IsNullOrWhiteSpace(Roles))
                msgs.Add("角色为<" + Roles + ">");
            var msg = string.Join("、", msgs);
            msg = "此页面需要" + msg + "的用户才能访问，请登录你的账号。";

            if (filterContext.HttpContext.Request.Url != null)
            {
                var returnUrl = filterContext.HttpContext.Request.Url.AbsolutePath;
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                var url = urlHelper.Action("Login", "Account", new { returnUrl = returnUrl });
                filterContext.Result = new RedirectResult(url).WithInfo(msg);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}