using System;
using System.Data.Entity;

namespace SdojJudger.Database
{
    public class JudgerDbContext : DbContext
    {
        public JudgerDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<SolutionEntity> SolutionEntities { get; set; }

        public static JudgerDbContext Create()
        {
            var db = new JudgerDbContext();
#if DEBUG
            db.Database.Log += Console.WriteLine;
#endif
            return db;
        }
    }
}
