using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using SdojWeb.Infrastructure.Extensions;
using System.Collections.Generic;

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

        public MvcHtmlString BuildAjaxUpdateA<TValue>(Expression<Func<TModel, TValue>> expression, string updateTarget)
        {
            if (updateTarget == null)
                throw new ArgumentNullException(nameof(updateTarget));

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

            var attr = new Dictionary<string, object>()
            {
                { "data-ajax-update", updateTarget },
                { "data-ajax-mode", "replace" },
                { "data-ajax", true }
            };

            return _html.ActionLink(displayName, _action, r, attr);
        }

        protected readonly HtmlHelper _html;

        protected readonly string _orderBy;

        protected readonly bool? _asc;

        protected readonly string _action;

        protected readonly RouteValueDictionary _route;
    }

    public class AjaxUpdateABuilder<TModel> : SortableThBuilder<TModel> where TModel : class
    {
        internal AjaxUpdateABuilder(HtmlHelper html, SortablePagedList<TModel> paged, string action, RouteValueDictionary route, string updateTarget)
            : base(html, paged, action, route)
        {
            if (updateTarget == null)
                throw new ArgumentNullException(nameof(updateTarget));

            _updateTarget = updateTarget;
        }

        public MvcHtmlString BuildAjaxUpdateA<TValue>(Expression<Func<TModel, TValue>> expression)
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

            var attr = new Dictionary<string, object>()
            {
                { "data-ajax-update", _updateTarget },
                { "data-ajax-mode", "replace" },
                { "data-ajax", "true" }
            };

            return _html.ActionLink(displayName, _action, r, attr);
        }

        private readonly string _updateTarget;
    }
}