using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;

namespace SdojWeb.Models
{
    public class ApplicationDbContext :
        IdentityDbContext
            <ApplicationUser, ApplicationRole, int, ApplicationUserLogin,
                ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>().Configure(x => x.HasColumnType("datetime2"));
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.CreateUser)
                .WithMany(u => u.Questions)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Question>()
                .HasRequired(q => q.SampleData)
                .WithMany(d => d.NoUseQuestions)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Solution> Solutions { get; set; }

        public DbSet<QuestionData> QuestionDatas { get; set; }

        public DbSet<SolutionLock> SolutionLocks { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}