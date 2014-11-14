using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using log4net;
using System.IO;

namespace SdojJudger.Database
{
    public class JudgerDbContext
    {
        private JudgerDbContext()
        {
            _db = GetDbConnection();
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

            EnsureConnectionOpen();

            using (var tran = (SqlCeTransaction)_db.BeginTransaction())
            {
                await _db.ExecuteAsync(deleteSql, new { Ids = dataList.Select(x => x.Id) }, tran);

                foreach (var data in dataList)
                {
                    var command = new SqlCeCommand(insertSql, (SqlCeConnection)_db, tran);
                    command.Parameters.AddWithValue("@Id", data.Id);
                    command.Parameters.AddWithValue("@Input", data.Input);
                    command.Parameters.AddWithValue("@Output", data.Output);
                    command.Parameters.AddWithValue("@MemoryLimitMb", data.MemoryLimitMb);
                    command.Parameters.AddWithValue("@TimeLimit", data.TimeLimit);
                    command.Parameters.AddWithValue("@UpdateTicks", data.UpdateTicks);

                    await command.ExecuteNonQueryAsync();
                }
                
                tran.Commit();
            }
        }

        private void EnsureConnectionOpen()
        {
            if (_db.State == ConnectionState.Broken)
            {
                _db.Close();
            }
            if (_db.State == ConnectionState.Closed)
            {
                _db.Open();
            }
        }

        public async Task Initialize()
        {
            EnsureDatabase();

            const string tableName = "QuestionDatas";

            bool tableExist = await TableExist(tableName);

            if (tableExist)
            {
                return;
            }

            await CreateTableNoCheck();
        }

        private void EnsureDatabase()
        {
            if (!File.Exists(_db.Database))
            {
                var engine = new SqlCeEngine(_db.ConnectionString);
                engine.CreateDatabase();
            }
        }

        private async Task CreateTableNoCheck()
        {
            EnsureConnectionOpen();
            
            var createSql = 
                "CREATE TABLE [QuestionDatas] (    \r\n" +
                "  [Id] int NOT NULL               \r\n" +
                ", [Input] ntext NULL              \r\n" +
                ", [Output] ntext NOT NULL         \r\n" +
                ", [MemoryLimitMb] real NOT NULL   \r\n" +
                ", [TimeLimit] int NOT NULL        \r\n" +
                ", [UpdateTicks] bigint NOT NULL   \r\n" +
                ");                                \r\n";

            var alterSql =
                "ALTER TABLE [QuestionDatas] ADD CONSTRAINT [PK_dbo.QuestionDatas] PRIMARY KEY ([Id]);";

            using (IDbTransaction tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(createSql);

                await _db.ExecuteAsync(alterSql);

                tran.Commit();
            }
        }

        private async Task<bool> TableExist(string tableName)
        {
            var sql = "SELECT 1                       " +
                      "FROM INFORMATION_SCHEMA.TABLES " +
                      "WHERE TABLE_NAME = @tableName  ";
            var exist = await _db.ExecuteScalarAsync<bool>(sql, new { tableName = tableName });
            return exist;
        }

        private SqlCeConnection GetDbConnection()
        {
            var db = new SqlCeConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            return db;
        }

        private SqlCeConnection _db;

        public static JudgerDbContext Create()
        {
            var db = new JudgerDbContext();
            var log = LogManager.GetLogger(typeof(JudgerDbContext));
            return db;
        }
    }
}