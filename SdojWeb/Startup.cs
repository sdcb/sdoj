using System.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SdojWeb.Controllers;

[assembly: OwinStartupAttribute(typeof(SdojWeb.Startup))]
namespace SdojWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            GlobalHost.DependencyResolver.UseSqlServer(connection);
            app.MapSignalR<JudgeConnection>("/SignalR/Judge");
        }
    }
}
