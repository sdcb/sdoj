using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Infrastructure.Identity
{
    public static class UserExtensions
    {
        public static bool IsUserOrAdmin(this IPrincipal user, string userId)
        {
            return 
                user.Identity.GetUserId() == userId || 
                user.IsInRole("admin");
        }
    }
}