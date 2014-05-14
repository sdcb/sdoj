using System.Linq;
using System.Security.Principal;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using SdojWeb.Models;

namespace SdojWeb.Infrastructure.Identity
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IIdentity _identity;
        private readonly ApplicationDbContext _context;

        private UsefulUserModel _user;

        public CurrentUser(IIdentity identity, ApplicationDbContext context)
        {
            _identity = identity;
            _context = context;
        }

        public UsefulUserModel User
        {
            get
            {
                var userId = _identity.GetIntUserId();
                if (_user != null) return _user;

                _user = _context.Users.Where(x => x.Id == userId)
                    .Project().To<UsefulUserModel>()
                    .FirstOrDefault();

                return _user;
            }
        }
    }
}