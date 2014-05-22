using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    // You can add profile data for the user by adding more properties to your CreateUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public sealed class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationUser()
        {
        }

        public ICollection<Question> Questions { get; set; }

        public ApplicationUser(string email)
        {
            Email = email;
            UserName = email;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class UsefulUserModel : IMapFrom<ApplicationUser>
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var manager =
                new ApplicationUserManager(
                    new UserStore
                        <ApplicationUser, ApplicationRole, int, ApplicationUserLogin,
                            ApplicationUserRole, ApplicationUserClaim>(context.Get<ApplicationDbContext>()));
            // 配置用户名的验证逻辑
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
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
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser, int>
            {
                Subject = "安全代码",
                BodyFormat = "你的安全代码为: {0}"
            });
            manager.EmailService = new EmailService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser, int>(
                        dataProtectionProvider.Create("ASP.NET Identity"));
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

    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public ApplicationRole()
        {
        }

        public ApplicationRole(string name)
        {
            Name = name;
        }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public ApplicationUser User { get; set; }

        public ApplicationRole Role { get; set; }
    }
    public class ApplicationUserClaim : IdentityUserClaim<int> { }
    public class ApplicationUserLogin : IdentityUserLogin<int> { }

    public class ApplicationRoleManager : RoleManager<ApplicationRole, int>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, int> store) : base(store)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var db = context.Get<ApplicationDbContext>();
            var store = new RoleStore<ApplicationRole, int, ApplicationUserRole>(db);
            var manager = new ApplicationRoleManager(store);
            return manager;
        }
    }

    public static class IdentityExtensions
    {
        public static int GetIntUserId(this IIdentity identity)
        {
            var uid = identity.GetUserId();
            int intuid;

            return int.TryParse(uid, out intuid) ? 
                intuid : 
                0;
        }
    }
}