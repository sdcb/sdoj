using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models
{
    public class User : IUser<int>
    {
        public User()
        {
            UserClaims = new HashSet<UserClaim>();
            UserLogins = new HashSet<UserLogin>();
            Questions = new HashSet<Question>();
            Solutions = new HashSet<Solution>();
            Roles = new HashSet<Role>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim(CustomClaims.EmailConfirmed, EmailConfirmed.ToString()));

            return userIdentity;
        }

        public int Id { get; set; }

        [StringLength(256), Index(IsUnique = true)]
        public string Email { get; set; }

        [Required, Index(IsUnique = true)]
        [StringLength(256)]
        public string UserName { get; set; }

        public bool EmailConfirmed { get; set; }

        [MaxLength(70), Column(TypeName = "varchar")]
        public string PasswordHash { get; set; }

        public Guid SecurityStamp { get; set; }

        [MaxLength(20), Column(TypeName = "varchar")]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public virtual ICollection<UserClaim> UserClaims { get; set; }

        public virtual ICollection<UserLogin> UserLogins { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<Solution> Solutions { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}