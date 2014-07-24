using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
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

        public static IHtmlString DeleteIcon(
            this HtmlHelper html, 
            object id, 
            string displayText = null, 
            string customClass = null)
        {
            var fmtTag = string.Format(
                "<a href='#' data-id='{0}' title='删除' class='glyphicon glyphicon-remove text-danger delete-link {1}'>{2}</a>",
                id, customClass, displayText);
            return html.Raw(fmtTag);
        }
    }
}