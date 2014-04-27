using Microsoft.AspNet.Identity.Owin;
using SdojWeb.Models;
using StructureMap.Configuration.DSL;
using System.Security.Principal;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.IoC
{
    public class MvcRegistry : Registry
    {
        public MvcRegistry()
        {
            For<BundleCollection>().Use(BundleTable.Bundles);
            For<RouteCollection>().Use(RouteTable.Routes);
            For<IIdentity>().Use(() => HttpContext.Current.User.Identity);
            For<HttpSessionStateBase>()
                .Use(() => new HttpSessionStateWrapper(HttpContext.Current.Session));
            For<HttpContextBase>()
                .Use(() => new HttpContextWrapper(HttpContext.Current));
            For<HttpServerUtilityBase>()
                .Use(() => new HttpServerUtilityWrapper(HttpContext.Current.Server));
            For<ApplicationUserManager>()
                .Use(() => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>());
            For<ApplicationRoleManager>()
                .Use(() => HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>());
        }
    }
}