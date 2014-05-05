using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Tasks;

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