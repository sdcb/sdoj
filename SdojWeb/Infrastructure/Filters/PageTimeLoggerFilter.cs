using System.Diagnostics;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.Filters
{
    public class PageTimeLoggerFilter : ActionFilterAttribute
    {
        private Stopwatch Stopwatch { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            filterContext.Controller.ViewData["_Stopwatch"] = Stopwatch;
            base.OnActionExecuting(filterContext);
        }
    }
}