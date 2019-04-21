using System.Diagnostics;
using System;
using System.Data.Entity;
using SdojWeb.Models.DbModels;
using System.Data.Entity.ModelConfiguration.Conventions;

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
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Properties<DateTime>().Configure(x => x.HasColumnType("datetime2"));

            modelBuilder.Entity<Question>()
                .HasRequired(q => q.CreateUser)
                .WithMany(u => u.Questions)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Solution>()
                .HasOptional(s => s.Lock)
                .WithRequired(l => l.Solution)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Question>()
                .HasOptional(q => q.Process2JudgeCode)
                .WithRequired(s => s.Question)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Contest>()
                .HasRequired(x => x.CreateUser)
                .WithMany(x => x.OwnedContests)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Solution>()
                .HasOptional(x => x.WrongAnswer)
                .WithRequired(x => x.Solution);
            
            modelBuilder.Entity<User>()
                .HasMany(x => x.Roles).WithMany(x => x.Users)
                .Map(x =>
                {
                    x.ToTable("UserRole");
                    x.MapLeftKey("UserId");
                    x.MapRightKey("RoleId");
                });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Solution> Solutions { get; set; }

        public DbSet<SolutionWrongAnswer> SolutionWrongAnswers { get; set; }

        public DbSet<QuestionData> QuestionDatas { get; set; }

        public DbSet<SolutionLock> SolutionLocks { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserClaim> UserClaims { get; set; }

        public DbSet<UserLogin> UserLogins { get; set; }

        public DbSet<QuestionGroup> QuestionGroups { get; set; }

        public DbSet<QuestionGroupItem> QuestionGroupItems { get; set; }

        public DbSet<Process2JudgeCode> Process2JudgeCode { get; set; }

        public DbSet<Contest> Contests { get; set; }

        public DbSet<ContestUser> ContestUsers { get; set; }

        public DbSet<ContestQuestion> ContestQuestions { get; set; }

        public DbSet<ContestSolution> ContestSolutions { get; set; }

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