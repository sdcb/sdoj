using System.Web.Mvc;
using System.Web.Routing;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class ControllerExtensions 
    {
        public static JavaScriptResult JavascriptRedirectToAction(this Controller me, string actionName)
        {
            var urlBuilder = new UrlHelper(me.Request.RequestContext, RouteTable.Routes);
            var url = urlBuilder.Action(actionName);
            var javascript = "location.href='" + url + "';";

            var result = new JavaScriptResult { Script = javascript };
            return result;
        }
    }
}