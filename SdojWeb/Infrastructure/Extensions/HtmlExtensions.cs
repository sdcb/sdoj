using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Web;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlString Json(this System.Web.Mvc.HtmlHelper helper, object obj)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new JsonConverter[]
                {
                    new StringEnumConverter(),
                },
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            return MvcHtmlString.Create(JsonConvert.SerializeObject(obj, settings));
        }
    }
}