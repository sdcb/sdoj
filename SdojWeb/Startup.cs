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
            app.MapSignalR<JudgeConnection>("/SignalR/Judge");
        }
    }
}
