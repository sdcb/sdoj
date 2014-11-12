using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using log4net;

namespace SdojJudger.Database
{
    public class JudgerDbContext
    {
        private JudgerDbContext()
        {
            _db = new SqlCeConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

        public async Task<IEnumerable<QuestionData>> FindDatasByIds(int[] ids)
        {
            const string sql = "SELECT * FROM QuestionDatas WHERE Id IN @Ids";
            var datas = await _db.QueryAsync<QuestionData>(sql, new { Ids = ids });
            return datas;
        }

        public async Task<IEnumerable<QuestionDataSummary>> FindDataSummarysByIdsInOrder(IEnumerable<int> ids)
        {
            const string sql = "SELECT Id, UpdateTicks " +
                               "FROM QuestionDatas WHERE Id IN @Ids " +
                               "ORDER BY Id";
            var datas = await _db.QueryAsync<QuestionDataSummary>(sql, new { Ids = ids });
            return datas;
        }

        public async Task DeleteAndCreateData(IEnumerable<QuestionData> datas)
        {
            var dataList = datas.ToList();
            const string deleteSql = "DELETE FROM QuestionDatas WHERE Id IN @Ids";
            const string insertSql = "INSERT INTO QuestionDatas VALUES " +
                                     "(@Id, @Input, @Output, @MemoryLimitMb, @TimeLimit, @UpdateTicks)";

            if (_db.State == ConnectionState.Broken)
            {
                _db.Close();
            }
            if (_db.State == ConnectionState.Closed)
            {
                _db.Open();
            }

            using (var tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(deleteSql, new { Ids = dataList.Select(x => x.Id) });
                await _db.ExecuteAsync(insertSql, dataList);
            }
        }

        private IDbConnection _db;

        public static JudgerDbContext Create()
        {
            var db = new JudgerDbContext();
#if DEBUG
            var log = LogManager.GetLogger(typeof(JudgerDbContext));
#endif
            return db;
        }
    }
}