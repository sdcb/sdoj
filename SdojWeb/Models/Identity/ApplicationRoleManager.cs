using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SdojWeb.Models.Identity;

namespace SdojWeb.Models
{
    public class ApplicationRoleManager : RoleManager<Role, int>
    {
        public ApplicationRoleManager(IRoleStore<Role, int> store)
            : base(store)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var db = context.Get<ApplicationDbContext>();
            var store = new RoleStore(db);
            var manager = new ApplicationRoleManager(store);
            return manager;
        }
    }
}