using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using SdojWeb.Models.Identity;

namespace SdojWeb.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<SdojWeb.Models.ApplicationDbContext>
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

            // 添加预定义用户。

            userManager.Create(new User {UserName="sdflysha@qq.com", Email = "sdflysha@qq.com", EmailConfirmed = true}, "A-Pa5sword-That:Never8eenUsed");
            userManager.Create(new User {UserName = "flysha@live.com", Email = "flysha@live.com", EmailConfirmed = true }, "A-Pa5sword-That:Never8eenUsed");
            userManager.Create(new User {UserName = "397482054@qq.com", Email = "397482054@qq.com", EmailConfirmed = false }, "A-Pa5sword-That:Never8eenUsed");

            // 添加预定义角色到用户。

            roleManager.Create(new Role {Name = SystemRoles.Admin});
            roleManager.Create(new Role {Name = SystemRoles.Judger});

            User user;

            user = userManager.FindByName("flysha@live.com");
            userManager.AddToRole(user.Id, SystemRoles.Admin);

            user = userManager.FindByName("sdflysha@qq.com");
            userManager.AddToRole(user.Id, SystemRoles.Judger);
        }
    }
}
