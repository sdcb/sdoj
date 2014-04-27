using System.Diagnostics;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.Filters
{
    public class PageTimeLoggerFilter : ActionFilterAttribute
    {
        private Stopwatch _Stopwatch { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();
            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            _Stopwatch.Stop();
            var text = string.Format("页面用时: {0}ms", _Stopwatch.Elapsed.TotalMilliseconds);
            filterContext.Controller.TempData["PageTime"] = text;
            base.OnResultExecuting(filterContext);
        }
    }
}