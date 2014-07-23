using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static int GetIntUserId(this IIdentity identity)
        {
            var uid = identity.GetUserId();
            int intuid;

            return int.TryParse(uid, out intuid) ?
                intuid :
                0;
        }
    }
}