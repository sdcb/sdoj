using System.Collections.Generic;
using System.Web.Optimization;

namespace SdojWeb
{
    public class BundleConfig
    {
        // 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.validate-localization.js").ForceOrdered());

            bundles.Add(new ScriptBundle("~/bundles/jqueryajax").Include(
                        "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/ko").Include(
                        "~/Scripts/knockout-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/bundles/bootstrap-markdown-css").Include(
                        "~/Content/bootstrap-markdown/bootstrap-markdown.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/marked").Include(
                        "~/Scripts/marked.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-markdown").Include(
                        "~/Scripts/bootstrap-markdown.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));
        }
    }

    internal static class BundleExtensions
    {
        public static Bundle ForceOrdered(this Bundle sb)
        {
            sb.Orderer = new AsIsBundleOrderer();
            return sb;
        }
    }

    internal class AsIsBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}
