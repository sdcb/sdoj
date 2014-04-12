using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace SdojWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    [DisplayColumn("UserName")]
    public sealed class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
        }

        public ApplicationUser(string email)
        {
            Email = email;
            UserName = email;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // 配置用户名的验证逻辑
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                
                RequireUniqueEmail = true
            };
            // 配置密码的验证逻辑
            manager.PasswordValidator = new PasswordValidator()
            {
                RequiredLength = 6, 
                RequireNonLetterOrDigit = false, 
                RequireDigit = false, 
                RequireLowercase = false, 
                RequireUppercase = false, 
            };
            // 注册双重身份验证提供程序。此应用程序使用手机和电子邮件作为接收用于验证用户的一个步骤
            // 你可以编写自己的提供程序并在此插入
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "安全代码", 
                BodyFormat = "你的安全代码为: {0}"
            });
            manager.EmailService = new EmailService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            const string mailfrom = "sdcb@live.cn";
            const string password = "***REMOVED***";

            var mail = new MailMessage(mailfrom, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };

            var client = new SmtpClient("smtp.live.com", 587)
            {
                Credentials = new NetworkCredential(mailfrom, password),
                EnableSsl = true,
            };

            await client.SendMailAsync(mail);
        }
    }
}