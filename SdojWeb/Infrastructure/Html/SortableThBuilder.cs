using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using SdojWeb.Infrastructure.Extensions;
using System.Collections.Generic;
using PagedList;
using System.Web;

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

        protected readonly HtmlHelper _html;

        protected readonly string _orderBy;

        protected readonly bool? _asc;

        protected readonly string _action;

        protected readonly RouteValueDictionary _route;
    }

    public interface IAjaxPagerContext
    {
        string GetUpdateTargetId();

        IPagedList GetPagedList();

        string AjaxPagerUrl(int page);

        string OnSuccessCallback { get; }
    }

    public class AjaxUpdateBuilder<TModel> : SortableThBuilder<TModel>, IAjaxPagerContext where TModel : class
    {
        internal AjaxUpdateBuilder(HtmlHelper html, SortablePagedList<TModel> paged, string action, RouteValueDictionary route, string updateTarget, string onSuccess)
            : base(html, paged, action, route)
        {
            if (updateTarget == null)
                throw new ArgumentNullException(nameof(updateTarget));

            _updateTarget = updateTarget;

            _ajaxAttr = new Dictionary<string, object>()
            {
                { "data-ajax-update", _updateTarget },
                { "data-ajax-mode", "replace" },
                { "data-ajax", "true" },
                { "data-ajax-method", "POST" }
            };

            _paged = paged;
            OnSuccessCallback = onSuccess;
        }

        public MvcHtmlString AjaxUpdateHeader<TValue>(Expression<Func<TModel, TValue>> expression)
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

            return _html.ActionLink(displayName, _action, r, _ajaxAttr);
        }

        public string AjaxPagerUrl(int page)
        {
            var r = new RouteValueDictionary(_route);
            r["page"] = page;

            var requestContext = _html.ViewContext.RequestContext;
            var url = new UrlHelper(requestContext);
            return url.Action(_action, r);
        }

        public IPagedList GetPagedList()
        {
            return _paged;
        }

        public string GetUpdateTargetId()
        {
            if (!_updateTarget.StartsWith("#"))
            {
                throw new InvalidOperationException();
            }
            return _updateTarget.TrimStart('#');
        }

        private readonly Dictionary<string, object> _ajaxAttr;

        public readonly string _updateTarget;

        private readonly SortablePagedList<TModel> _paged;

        public string OnSuccessCallback { get; private set; }
    }
}