using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using PagedList;
using SdojWeb.Infrastructure.Html;
using HtmlHelper = System.Web.Mvc.HtmlHelper;

namespace SdojWeb.Infrastructure.Extensions
{
    public class SortablePagedList<T> : PagedList<T>
    {
        public SortablePagedList(IQueryable<T> superset, int pageNumber, int pageSize) : base(superset, pageNumber, pageSize)
        {
        }

        public SortablePagedList(IEnumerable<T> superset, int pageNumber, int pageSize) : base(superset, pageNumber, pageSize)
        {
        }

        public string OrderBy { get; set; }

        public bool? Asc { get; set; }

        public SortableThBuilder<T> GetThBuilder(HtmlHelper htmlHelper, string actionName, RouteValueDictionary route)
        {
            var builder = new SortableThBuilder<T>(htmlHelper, this, actionName, route);
            return builder;
        }
    }
}