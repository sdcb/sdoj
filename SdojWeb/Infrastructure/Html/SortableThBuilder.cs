using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using SdojWeb.Infrastructure.Extensions;

namespace SdojWeb.Infrastructure.Html
{
    public class SortableThBuilder<TModel> where TModel : class
    {
        internal SortableThBuilder(HtmlHelper html, SortablePagedList<TModel> paged, string action, RouteValueDictionary route)
        {
            _html = html;
            _orderBy = paged.OrderBy;
            _asc = paged.Asc;
            _action = action;
            _route = route;
        }

        public MvcHtmlString BuildA<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var memberName = ((MemberExpression)expression.Body).Member.Name;
            var metadataProvider = DependencyResolver.Current.GetService<ModelMetadataProvider>();
            var metadata = metadataProvider.GetMetadataForProperty(null, typeof(TModel), memberName);
            var displayName = metadata.DisplayName;

            var asc = _asc;
            if (_orderBy == memberName)
            {
                var direction = asc == null ? "" : (asc.Value ? "↓" : "↑");
                displayName = displayName + direction;
                asc = !asc;
            }
            else
            {
                asc = true;
            }

            var r = new RouteValueDictionary(_route);
            r["orderBy"] = memberName;
            r["asc"] = asc;

            return _html.ActionLink(displayName, _action, r);
        }

        private readonly HtmlHelper _html;

        private readonly string _orderBy;

        private readonly bool? _asc;

        private readonly string _action;

        private readonly RouteValueDictionary _route;
    }
}