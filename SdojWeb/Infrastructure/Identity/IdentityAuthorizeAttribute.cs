using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Alerts;

namespace SdojWeb.Infrastructure.Identity
{
    public class IdentityAuthorizeAttribute : AuthorizeAttribute
    {
        public bool EmailAuthorize { get; set; }

        public IdentityAuthorizeAttribute() 
        {
            EmailAuthorize = true;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (EmailAuthorize)
            {
                var currentUser = DependencyResolver.Current.GetService<ICurrentUser>();
                var user = currentUser.User;
                if (user == null || !currentUser.User.EmailConfirmed)
                {
                    return false;
                }
            }
            return base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var msgs = new List<string>();
            if (EmailAuthorize)
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
                var encodedReturnUrl = HttpUtility.UrlEncode(returnUrl);
                var url = "~/Account/Login?ReturnUrl=" + encodedReturnUrl;
                filterContext.Result = new RedirectResult(url).WithInfo(msg);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}