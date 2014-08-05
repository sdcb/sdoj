using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum v)
        {
            var field = v.GetType().GetField(v.ToString());
            if (field == null) return null;

            var display = ((DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), false)).FirstOrDefault();
            return display != null
                ? display.Name
                : v.ToString();
        }
    }
}