using System.Collections.Generic;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.Alerts
{
    public static class AlertExtensions
    {
        const string Alerts = "_Alerts";

        public static List<Alert> GetAlerts(this TempDataDictionary tempData)
        {
            if (!tempData.ContainsKey(Alerts))
            {
                tempData[Alerts] = new List<Alert>();
            }

            return (List<Alert>)tempData[Alerts];
        }

        public static ActionResult WithSuccess(this ActionResult result, string text)
        {
            return new AlertDecoratorResult(result, "alert-success", text);
        }

        public static ActionResult WithInfo(this ActionResult result, string text)
        {
            return new AlertDecoratorResult(result, "alert-info", text);
        }

        public static ActionResult WithWarning(this ActionResult result, string text)
        {
            return new AlertDecoratorResult(result, "alert-warning", text);
        }

        public static ActionResult WithError(this ActionResult result, string text)
        {
            return new AlertDecoratorResult(result, "alert-danger", text);
        }

        public static JavaScriptResult WithJavascriptAlert(this JavaScriptResult result, string text)
        {
            result.Script = "alert('" + text + "');" + result.Script;
            return result;
        }
    }
}