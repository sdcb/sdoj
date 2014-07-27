using System.Diagnostics;
using System;
using System.Data.Entity;

namespace SdojWeb.Models
{
    public class ApplicationDbContext : DbContext
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
                .HasOptional(q => q.SampleData)
                .WithMany();

            modelBuilder.Entity<Solution>()
                .HasOptional(s => s.Lock)
                .WithRequired(l => l.Solution)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Solution> Solutions { get; set; }

        public DbSet<QuestionData> QuestionDatas { get; set; }

        public DbSet<SolutionLock> SolutionLocks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserClaim> UserClaims { get; set; }

        public DbSet<UserLogin> UserLogins { get; set; }

        public static ApplicationDbContext Create()
        {
            var db = new ApplicationDbContext();

#if DEBUG
            db.Database.Log += s => Debug.WriteLine(s);
#endif

            return db;
        }
    }
}