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
            Expression<Func<TModel, TValue>> expression, string actionName,  string orderBy, bool? asc)
        {
            var displayName = html.DisplayNameFor(expression).ToString();
            var memberName = ((MemberExpression) expression.Body).Member.Name;

            if (orderBy == memberName)
            {
                var direction = asc == null ? "" : (asc.Value ? "↓" : "↑");
                displayName = displayName + direction;
                asc = !asc;
            }
            else
            {
                asc = true;
            }
            return html.ActionLink(displayName, actionName, new { orderBy = memberName, asc });
        }

        public static MvcHtmlString EnumDropDownList<TEnum>(this HtmlHelper html, string name, TEnum? value, string optionalLabel, object htmlAttribute) where TEnum:struct
        {
            var optionalItem = new SelectListItem {Value = "", Text = optionalLabel};
            var selectList = new List<SelectListItem> {optionalItem};
            selectList.AddRange(EnumHelper.GetSelectList(typeof (TEnum)));

            if (value == null)
            {
                selectList[0].Selected = true;
                return html.DropDownList(name, selectList, htmlAttribute);
            }

            bool finded = false;

            for (var i = 1; i < selectList.Count; ++i)
            {
                var item = selectList[i];
                if (item.Value == value.ToString())
                {
                    item.Selected = true;
                    finded = true;
                }
            }

            if (!finded)
            {
                selectList[0].Selected = true;
                return html.DropDownList(name, selectList, htmlAttribute);
            }

            return html.DropDownList(name, selectList, htmlAttribute);
        }
    }
}