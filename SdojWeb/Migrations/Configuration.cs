using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Models.Identity;

namespace SdojWeb.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "SdojWeb.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            var roleStore = new RoleStore(context);
            var userManager = new ApplicationUserManager(new UserStore(context));
            var roleManager = new RoleManager<Role, int>(roleStore);

            // 添加预定义角色到用户。

            roleManager.Create(new Role { Name = SystemRoles.UserAdmin });
            roleManager.Create(new Role { Name = SystemRoles.QuestionAdmin });
            roleManager.Create(new Role { Name = SystemRoles.QuestionCreator });
            roleManager.Create(new Role { Name = SystemRoles.Judger });
            roleManager.Create(new Role { Name = SystemRoles.SolutionViewer });
            roleManager.Create(new Role { Name = SystemRoles.QuestionGroupAdmin });
            roleManager.Create(new Role { Name = SystemRoles.QuestionGroupCreator });

            // 添加预定义用户。

            userManager.Create(new User { UserName="qa", Email = "sdoj-question-admin@sdcb.in", EmailConfirmed = true}, "***REMOVED***");
            userManager.Create(new User { UserName = "ua", Email = "sdoj-user-admin@sdcb.in", EmailConfirmed = true }, "***REMOVED***");
            userManager.Create(new User { UserName = "qc", Email = "sdoj-question-creator@sdcb.in", EmailConfirmed = true }, "***REMOVED***");
            userManager.Create(new User { UserName = "j", Email = "sdoj-judger@sdcb.in", EmailConfirmed = true }, "***REMOVED***");

            userManager.Create(new User { UserName = "uc", Email = "sdoj-user-confirmed@sdcb.in", EmailConfirmed =  true }, "***REMOVED***");
            userManager.Create(new User { UserName = "u", Email = "sdoj-user@sdcb.in", EmailConfirmed = false }, "***REMOVED***");
            
            // 将预定义用户添加到角色

            User user = userManager.FindByName("qa");
            userManager.AddToRole(user.Id, SystemRoles.QuestionAdmin);

            user = userManager.FindByName("ua");
            userManager.AddToRole(user.Id, SystemRoles.UserAdmin);

            user = userManager.FindByName("qc");
            userManager.AddToRole(user.Id, SystemRoles.QuestionCreator);

            user = userManager.FindByName("j");
            userManager.AddToRole(user.Id, SystemRoles.Judger);
        }
    }
}
