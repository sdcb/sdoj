using System.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SdojWeb.Startup))]
namespace SdojWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // SignalR Scale Out。
            //var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //GlobalHost.DependencyResolver.UseSqlServer(connection);

            app.MapSignalR();
        }
    }
}
