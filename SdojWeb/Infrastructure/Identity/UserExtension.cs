using System.Security.Principal;
using SdojWeb.Models;

namespace SdojWeb.Infrastructure.Identity
{
    public static class UserExtensions
    {
        public static bool IsUserOrAdmin(this IPrincipal user, int userId)
        {
            return 
                user.Identity.GetIntUserId() == userId || 
                user.IsInRole("admin");
        }
    }
}