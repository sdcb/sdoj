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
        }
    }
}
