using SdojWeb.Infrastructure.Filters;
using System.Web;
using System.Web.Mvc;

namespace SdojWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ShowUserIsConfirmedFilter());
            filters.Add(new PageTimeLoggerFilter());
        }
    }
}
