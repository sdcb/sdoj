using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using SdojWeb.Models;

namespace SdojWeb.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SdojWeb.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "SdojWeb.Models.ApplicationDbContext";
        }

        protected override async void Seed(SdojWeb.Models.ApplicationDbContext context)
        {
            var userStore = new UserStore<ApplicationUser>(context);
            var roleStore = new RoleStore<IdentityRole>(context);
            var userManager = new ApplicationUserManager(userStore);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            await userManager.CreateAsync(new ApplicationUser("sdflysha@qq.com") {EmailConfirmed = true}, "A-Pa5sword-That:Never8eenUsed");
            await userManager.CreateAsync(new ApplicationUser("397482054@qq.com") { EmailConfirmed = false }, "A-Pa5sword-That:Never8eenUsed");
            await roleManager.CreateAsync(new IdentityRole("admin"));
            await userManager.CreateAsync(new ApplicationUser("flysha@live.com") { EmailConfirmed = true }, "A-Pa5sword-That:Never8eenUsed");

            var user = await userManager.FindByNameAsync("flysha");
            await userManager.AddToRoleAsync(user.Id, "admin");
        }
    }
}
