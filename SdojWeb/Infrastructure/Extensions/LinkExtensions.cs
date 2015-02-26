using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
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
            selectList.AddRange(GetSelectList(typeof (TEnum)));

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

        private static IEnumerable<SelectListItem> GetSelectList(Type type)
        {
            IList<SelectListItem> selectList = new List<SelectListItem>();

            // According to HTML5: "The first child option element of a select element with a required attribute and
            // without a multiple attribute, and whose size is "1", must have either an empty value attribute, or must
            // have no text content."  SelectExtensions.DropDownList[For]() methods often generate a matching
            // <select/>.  Empty value for Nullable<T>, empty text for round-tripping an unrecognized value, or option
            // label serves in some cases.  But otherwise, ignoring this does not cause problems in either IE or Chrome.
            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            if (checkedType != type)
            {
                // Underlying type was non-null so handle Nullable<T>; ensure returned list has a spot for null
                selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
            }

            // Populate the list
            const BindingFlags bindingFlags =
                BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static;
            foreach (FieldInfo field in checkedType.GetFields(bindingFlags))
            {
                selectList.Add(new SelectListItem
                {
                    Text = GetDisplayName(field),
                    Value = field.GetRawConstantValue().ToString(),
                });
            }

            return selectList;

        }

        // Return non-empty name specified in a [Display] attribute for the given field, if any; field's name otherwise
        private static string GetDisplayName(FieldInfo field)
        {
            var display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                string name = display.GetName();
                if (!String.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return field.Name;
        }
    }
}