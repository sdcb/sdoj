using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
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
                    //OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, int>(
                    OnValidateIdentity = OnValidateIdentity<ApplicationUserManager, ApplicationUser, int>(
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

        /// <summary>
        ///     Can be used as the ValidateIdentity method for a CookieAuthenticationProvider which will check a user's security
        ///     stamp after validateInterval
        ///     Rejects the identity if the stamp changes, and otherwise will call regenerateIdentity to sign in a new
        ///     ClaimsIdentity
        /// 
        ///     copy from: http://www.symbolsource.org/MyGet/Metadata/aspnetwebstacknightly/Project/Microsoft.AspNet.Identity.Owin/2.0.0-rtm-140327/Release/Default/Microsoft.AspNet.Identity.Owin/Microsoft.AspNet.Identity.Owin/SecurityStampValidator.cs?ImageName=Microsoft.AspNet.Identity.Owin
        /// 
        /// </summary>
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity<TManager, TUser, TKey>(
            TimeSpan validateInterval, Func<TManager, TUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
            Func<ClaimsIdentity, TKey> getUserIdCallback)
            where TManager : UserManager<TUser, TKey>
            where TUser : class, IUser<TKey>
            where TKey : IEquatable<TKey>
        {
            return async context =>
            {
                var currentUtc = DateTimeOffset.UtcNow;
                if (context.Options != null && context.Options.SystemClock != null)
                {
                    currentUtc = context.Options.SystemClock.UtcNow;
                }
                var issuedUtc = context.Properties.IssuedUtc;

                // Only validate if enough time has elapsed
                var validate = (issuedUtc == null);
                if (issuedUtc != null)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    validate = timeElapsed > validateInterval;
                }
                if (validate)
                {
                    var manager = context.OwinContext.GetUserManager<TManager>();
                    var userId = getUserIdCallback(context.Identity);
                    if (manager != null && userId != null)
                    {
                        var user = await manager.FindByIdAsync(userId).ConfigureAwait(false);
                        var reject = true;
                        // Refresh the identity if the stamp matches, otherwise reject
                        if (user != null && manager.SupportsUserSecurityStamp)
                        {
                            var securityStamp =
                                context.Identity.FindFirstValue(Constants.DefaultSecurityStampClaimType);
                            if (securityStamp == await manager.GetSecurityStampAsync(userId).ConfigureAwait(false))
                            {
                                reject = false;
                                // Regenerate fresh claims if possible and resign in
                                if (regenerateIdentityCallback != null)
                                {
                                    var identity = await regenerateIdentityCallback.Invoke(manager, user);
                                    if (identity != null)
                                    {
                                        var authProp = new AuthenticationProperties
                                        {
                                            IsPersistent = context.Properties.IsPersistent
                                        };

                                        context.OwinContext.Authentication.SignIn(authProp, identity);
                                    }
                                }
                            }
                        }
                        if (reject)
                        {
                            context.RejectIdentity();
                            context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                        }
                    }
                }
            };
        }

    }
}