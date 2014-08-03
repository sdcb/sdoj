using System.Data.Entity;
using log4net;

namespace SdojJudger.Database
{
    public class JudgerDbContext : DbContext
    {
        public JudgerDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<QuestionData> Datas { get; set; }

        public static JudgerDbContext Create()
        {
            var db = new JudgerDbContext();
#if DEBUG
            var log = LogManager.GetLogger(typeof (JudgerDbContext));
            db.Database.Log += log.Debug;
#endif
            return db;
        }
    }
}