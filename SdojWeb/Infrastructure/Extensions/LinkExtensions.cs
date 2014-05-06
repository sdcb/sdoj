using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class LinkExtensions
    {
        public static MvcHtmlString SortableHeaderFor<TModel, TValue>(
            this HtmlHelper<IEnumerable<TModel>> html, 
            Expression<Func<TModel, TValue>> expression, string actionName,  string orderBy, bool asc)
        {
            var displayName = html.DisplayNameFor(expression).ToString();
            var memberName = ((MemberExpression) expression.Body).Member.Name;

            if (orderBy == memberName)
            {
                displayName = displayName + (asc ? '↓' : '↑');
                asc = !asc;
            }
            else
            {
                asc = true;
            }
            return html.ActionLink(displayName, actionName, new { orderBy = memberName, asc });
        }
    }
}