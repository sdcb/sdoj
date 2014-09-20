using System.Web.Routing;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class RouteValueDictionaryExtensions
    {
        public static RouteValueDictionary CopySetPaged(this RouteValueDictionary route, int page, string orderBy, bool? asc)
        {
            var r = new RouteValueDictionary(route);
            r["page"] = page;
            r["orderBy"] = orderBy;
            r["asc"] = asc;
            return r;
        }
    }
}