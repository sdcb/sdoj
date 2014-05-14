using System.Diagnostics;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SdojWeb.Models;

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
            var roleStore = new RoleStore<ApplicationRole, int, ApplicationUserRole>(context);
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>(context));
            var roleManager = new RoleManager<ApplicationRole, int>(roleStore);

            userManager.Create(new ApplicationUser("sdflysha@qq.com") {EmailConfirmed = true}, "A-Pa5sword-That:Never8eenUsed");
            userManager.Create(new ApplicationUser("397482054@qq.com") { EmailConfirmed = false }, "A-Pa5sword-That:Never8eenUsed");
            roleManager.Create(new ApplicationRole("admin"));
            userManager.Create(new ApplicationUser("flysha@live.com") { EmailConfirmed = true }, "A-Pa5sword-That:Never8eenUsed");

            var user = userManager.FindByName("flysha@live.com");
            userManager.AddToRole(user.Id, "admin");
        }
    }
}
