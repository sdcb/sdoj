using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using StructureMap.Query;

namespace SdojWeb.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Solution> Solutions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Solution>().Property(x => x.SubmitTime)
                .HasColumnType("datetime2")
                .HasPrecision(2);
            base.OnModelCreating(modelBuilder);
        }
    }
}