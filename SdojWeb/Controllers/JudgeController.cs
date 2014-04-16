using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class JudgeController : ApiController
    {
        private readonly ApplicationUserManager _userManager;

        public JudgeController()
        {
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        [HttpGet]
        public async Task Login(string username, string password)
        {
            var user = await _userManager.FindAsync(username, password);
            if (user != null)
            {
                await SignInAsync(user, false);
            }
        }

        [HttpGet]
        public int Sum(int a, int b)
        {
            return a + b;
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return 
                    HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(
                new AuthenticationProperties() { IsPersistent = isPersistent }, 
                await user.GenerateUserIdentityAsync(_userManager));
        }
    }
}
