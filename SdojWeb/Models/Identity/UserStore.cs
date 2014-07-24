using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class UserStore :
        IQueryableUserStore<User, int>, IUserPasswordStore<User, int>, IUserLoginStore<User, int>,
        IUserClaimStore<User, int>, IUserRoleStore<User, int>, IUserSecurityStampStore<User, int>,
        IUserEmailStore<User, int>, IUserPhoneNumberStore<User, int>, IUserTwoFactorStore<User, int>,
        IUserLockoutStore<User, int>
    {
        private readonly ApplicationDbContext db;


        public UserStore(ApplicationDbContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }


            this.db = db;
        }


        //// IQueryableUserStore<User, int>


        public IQueryable<User> Users
        {
            get { return db.Users; }
        }


        //// IUserStore<User, Key>


        public Task CreateAsync(User user)
        {
            db.Users.Add(user);
            return db.SaveChangesAsync();
        }


        public Task DeleteAsync(User user)
        {
            db.Users.Remove(user);
            return db.SaveChangesAsync();
        }


        public Task<User> FindByIdAsync(int userId)
        {
            return db.Users
                .Include(u => u.UserLogins).Include(u => u.Roles).Include(u => u.UserClaims)
                .FirstOrDefaultAsync(u => u.Id.Equals(userId));
        }


        public Task<User> FindByNameAsync(string userName)
        {
            return db.Users
                .Include(u => u.UserClaims).Include(u => u.Roles).Include(u => u.UserClaims)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }


        public Task UpdateAsync(User user)
        {
            db.Entry(user).State = EntityState.Modified;
            return db.SaveChangesAsync();
        }


        //// IUserPasswordStore<User, Key>


        public Task<string> GetPasswordHashAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.PasswordHash);
        }


        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }


        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }


        //// IUserLoginStore<User, Key>


        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (login == null)
            {
                throw new ArgumentNullException("login");
            }


            var userLogin = new UserLogin
            {
                UserId = user.Id,
                LoginProvider = login.ProviderKey,
                ProviderKey = login.ProviderKey
            };
            user.UserLogins.Add(userLogin);
            return Task.FromResult(0);
        }


        public async Task<User> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }


            var provider = login.LoginProvider;
            var key = login.ProviderKey;


            var userLogin = await db.UserLogins.FirstOrDefaultAsync(l => l.LoginProvider == provider && l.ProviderKey == key);


            if (userLogin == null)
            {
                return default(User);
            }


            return await db.Users
                .Include(u => u.UserLogins).Include(u => u.Roles).Include(u => u.UserClaims)
                .FirstOrDefaultAsync(u => u.Id.Equals(userLogin.UserId));
        }


        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult<IList<UserLoginInfo>>(user.UserLogins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList());
        }


        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (login == null)
            {
                throw new ArgumentNullException("login");
            }


            var provider = login.LoginProvider;
            var key = login.ProviderKey;


            var item = user.UserLogins.SingleOrDefault(l => l.LoginProvider == provider && l.ProviderKey == key);


            if (item != null)
            {
                user.UserLogins.Remove(item);
            }


            return Task.FromResult(0);
        }


        //// IUserClaimStore<User, int>


        public Task AddClaimAsync(User user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }


            var item = new UserClaim
            {
                UserId = user.Id,
                Type = claim.Type,
                Value = claim.Value
            };
            user.UserClaims.Add(item);
            return Task.FromResult(0);
        }


        public Task<IList<Claim>> GetClaimsAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult<IList<Claim>>(user.UserClaims.Select(c => new Claim(c.Type, c.Value)).ToList());
        }


        public Task RemoveClaimAsync(User user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }


            foreach (var item in user.UserClaims.Where(uc => uc.Value == claim.Value && uc.Type == claim.Type).ToList())
            {
                user.UserClaims.Remove(item);
            }


            foreach (var item in db.UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.Value == claim.Type && uc.Type == claim.Value).ToList())
            {
                db.UserClaims.Remove(item);
            }


            return Task.FromResult(0);
        }


        //// IUserRoleStore<User, int>


        public Task AddToRoleAsync(User user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }


            var userRole = db.Roles.SingleOrDefault(r => r.Name == roleName);


            if (userRole == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "ÕÒ²»µ½½ÇÉ«{0}¡£", new object[] { roleName }));
            }


            user.Roles.Add(userRole);
            return Task.FromResult(0);
        }


        public Task<IList<string>> GetRolesAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult<IList<string>>(user.Roles.Join(db.Roles, ur => ur.Id, r => r.Id, (ur, r) => r.Name).ToList());
        }


        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }


            return
                Task.FromResult(
                    db.Roles.Any(r => r.Name == roleName && r.Users.Any(u => u.Id.Equals(user.Id))));
        }


        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }


            var userRole = user.Roles.SingleOrDefault(r => r.Name == roleName);


            if (userRole != null)
            {
                user.Roles.Remove(userRole);
            }


            return Task.FromResult(0);
        }


        //// IUserSecurityStampStore<User, int>


        public Task<string> GetSecurityStampAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.SecurityStamp.ToString());
        }


        public Task SetSecurityStampAsync(User user, string stamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.SecurityStamp = new Guid(stamp);
            return Task.FromResult(0);
        }


        //// IUserEmailStore<User, int>


        public Task<User> FindByEmailAsync(string email)
        {
            return db.Users
                .Include(u => u.UserLogins).Include(u => u.Roles).Include(u => u.UserClaims)
                .FirstOrDefaultAsync(u => u.Email == email);
        }


        public Task<string> GetEmailAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.Email);
        }


        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.EmailConfirmed);
        }


        public Task SetEmailAsync(User user, string email)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.Email = email;
            return Task.FromResult(0);
        }


        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }


        //// IUserPhoneNumberStore<User, int>


        public Task<string> GetPhoneNumberAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.PhoneNumber);
        }


        public Task<bool> GetPhoneNumberConfirmedAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.PhoneNumberConfirmed);
        }


        public Task SetPhoneNumberAsync(User user, string phoneNumber)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }


        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }


        //// IUserTwoFactorStore<User, int>


        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.TwoFactorEnabled);
        }


        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }


        //// IUserLockoutStore<User, int>


        public Task<int> GetAccessFailedCountAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.AccessFailedCount);
        }


        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(user.LockoutEnabled);
        }


        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            return Task.FromResult(
                user.LockoutEndDateUtc.HasValue ?
                    new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) :
                    new DateTimeOffset());
        }


        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }


        public Task ResetAccessFailedCountAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }


        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }


        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }


            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.UtcDateTime);
            return Task.FromResult(0);
        }


        //// IDisposable


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
        }
    }
}