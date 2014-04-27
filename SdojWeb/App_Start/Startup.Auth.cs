using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using SdojWeb.Models;

namespace SdojWeb
{
    public partial class Startup
    {
        // 有关配置身份验证的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // 将数据库上下文和用户管理器配置为每个请求使用单个实例
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            // 使应用程序可以使用 Cookie 来存储已登录用户的信息
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, int>(
                        validateInterval: TimeSpan.FromMinutes(20),
                        regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager), 
                        getUserIdCallback: identity => int.Parse(identity.GetUserId()))
                }
            });
            // 使用 Cookie 临时存储有关某个用户使用第三方登录提供程序进行登录的信息
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // 取消注释以下行可允许使用第三方登录提供程序登录
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            app.UseGoogleAuthentication();
        }
    }
}