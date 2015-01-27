using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Infrastructure.Identity
{
    public static class UserExtensions
    {
        public static bool IsUserOrRole(this IPrincipal user, int userId, string role)
        {
            return
                user.Identity.GetUserId<int>() == userId ||
                user.IsInRole(role);
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