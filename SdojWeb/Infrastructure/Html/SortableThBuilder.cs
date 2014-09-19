using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using SdojWeb.Infrastructure.Extensions;

namespace SdojWeb.Infrastructure.Html
{
    public class SortableThBuilder<TModel>
    {
        public SortableThBuilder(HtmlHelper html, SortablePagedList<TModel> paged, string action)
        {
            _html = html;
            _orderBy = paged.OrderBy;
            _asc = paged.Asc;
            _action = action;
        }

        public MvcHtmlString Build<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var displayName = _html.DisplayName(expression.Name).ToString();
            var memberName = ((MemberExpression)expression.Body).Member.Name;

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

            return _html.ActionLink(displayName, _action, new { orderBy = memberName, asc });
        }

        private readonly HtmlHelper _html;

        private readonly string _orderBy;

        private readonly bool? _asc;

        private readonly string _action;
    }
}