using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Tasks;

namespace SdojWeb.Infrastructure.Filters
{
    public class PageTimeLoggerFilter : ActionFilterAttribute
    {
        private Stopwatch _Stopwatch { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();
            filterContext.Controller.ViewData["_Stopwatch"] = _Stopwatch;
            base.OnActionExecuting(filterContext);
        }
    }
}