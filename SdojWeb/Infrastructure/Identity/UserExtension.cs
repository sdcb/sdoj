using System;
using System.Security.Claims;
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

        public static bool EmailConfirmed(this IPrincipal user)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal == null) return false;

            var claim = claimsPrincipal.FindFirst(CustomClaims.EmailConfirmed);
            var confirmed = Convert.ToBoolean(claim.Value);
            return confirmed;
        }
    }
}