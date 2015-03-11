using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using log4net;
using System.IO;
using System;
using SdojJudger.Models;

namespace SdojJudger.Database
{
    public class JudgerDbContext : IDisposable
    {
        public JudgerDbContext()
        {
            _db = new SqlCeConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

        public async Task<IEnumerable<QuestionData>> FindDatasByIds(int[] ids)
        {
            const string sql = "SELECT * FROM QuestionDatas WHERE Id IN @Ids";
            var datas = await _db.QueryAsync<QuestionData>(sql, new { Ids = ids });
            return datas;
        }

        public async Task<IEnumerable<DbHashModel>> FindDataSummarysByIdsInOrder(IEnumerable<int> ids)
        {
            const string sql = "SELECT Id, UpdateTicks " +
                               "FROM QuestionDatas WHERE Id IN @Ids " +
                               "ORDER BY Id";
            var datas = await _db.QueryAsync<DbHashModel>(sql, new { Ids = ids });
            return datas;
        }

        public async Task<DbHashModel> FindProcess2HashById(int questionId)
        {
            const string sql = "SELECT Id, UpdateTicks " + 
                               "FROM QuestionP2Code WHERE Id = @id ";
            var data = await _db.ExecuteScalarAsync<DbHashModel>(sql, new { Id = questionId });
            return data;
        }

        public async Task<bool> ContainsProcess2Code(int questionId)
        {
            const string sql = "SELECT COUNT(*) FROM QuestionP2Code WHERE Id = @id ";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { Id = questionId });
            return count > 0;
        }

        public async Task<QuestionP2Code> FindProcess2CodeById(int questionId)
        {
            const string sql = "SELECT * FROM QuestionP2Code WHERE Id = @id";
            var data = await _db.ExecuteScalarAsync<QuestionP2Code>(sql, new { id = questionId });
            return data;
        }

        public async Task DeleteAndCreateProcess2Code(QuestionP2Code data)
        {
            const string deleteSql = "DELETE FROM QuestionP2Code WHERE Id = @id ";
            const string insertSql = "INSERT INTO QuestionP2Code VALUES " + 
                                     "(@QuestionId, @Code, @Language, @RunTimes, @TimeLimitMs, @MemoryLimitMb, @UpdateTicks)";

            EnsureConnectionOpen();

            using (var tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(deleteSql, new { id = data.QuestionId });

                var command = new SqlCeCommand(insertSql, _db, tran);
                command.Parameters.AddWithValue("@QuestionId", data.QuestionId);
                command.Parameters.AddWithValue("@Code", data.Code);
                command.Parameters.AddWithValue("@Language", data.Language);
                command.Parameters.AddWithValue("@RunTimes", data.RunTimes);
                command.Parameters.AddWithValue("@TimeLimitMs", data.TimeLimitMs);
                command.Parameters.AddWithValue("@MemoryLimitMb", data.MemoryLimitMb);
                command.Parameters.AddWithValue("@UpdateTicks", data.UpdateTicks);

                await command.ExecuteNonQueryAsync();

                tran.Commit();
            }
        }

        public async Task DeleteAndCreateData(IEnumerable<QuestionData> datas)
        {
            var dataList = datas.ToList();
            const string deleteSql = "DELETE FROM QuestionDatas WHERE Id IN @Ids";
            const string insertSql = "INSERT INTO QuestionDatas VALUES " +
                                     "(@Id, @Input, @Output, @MemoryLimitMb, @TimeLimit, @UpdateTicks)";

            EnsureConnectionOpen();

            using (var tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(deleteSql, new { Ids = dataList.Select(x => x.Id) }, tran);

                foreach (var data in dataList)
                {
                    var command = new SqlCeCommand(insertSql, _db, tran);
                    command.Parameters.AddWithValue("@Id", data.Id);
                    command.Parameters.AddWithValue("@Input", data.Input ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Output", data.Output ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MemoryLimitMb", data.MemoryLimitMb);
                    command.Parameters.AddWithValue("@TimeLimit", data.TimeLimit);
                    command.Parameters.AddWithValue("@UpdateTicks", data.UpdateTicks);

                    await command.ExecuteNonQueryAsync();
                }
                
                tran.Commit();
            }
        }

        protected void EnsureConnectionOpen()
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

        protected SqlCeConnection _db;

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).          
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                _db.Dispose();
                // set large fields to null.
                _db = null;

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        ~JudgerDbContext()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class DbInitializer : JudgerDbContext
    {
        public async Task Initialize()
        {
            if (!File.Exists(_db.Database))
            {
                var engine = new SqlCeEngine(_db.ConnectionString);
                engine.CreateDatabase();
            }

            EnsureConnectionOpen();
            if (!await TableExist("QuestionDatas"))
            {
                await CreateTableQuestionData();
            }
            if (!await TableExist("QuestionP2Code"))
            {
                await CreateTableP2Code();
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

        private async Task CreateTableQuestionData()
        {
            var createSql =
                "CREATE TABLE [QuestionDatas] (  \r\n" +
                "  [Id] int NOT NULL             \r\n" +
                ", [Input] ntext NULL            \r\n" +
                ", [Output] ntext NOT NULL       \r\n" +
                ", [MemoryLimitMb] real NOT NULL \r\n" +
                ", [TimeLimit] int NOT NULL      \r\n" +
                ", [UpdateTicks] bigint NOT NULL \r\n" +
                ");                              \r\n";

            var alterSql =
                "ALTER TABLE [QuestionDatas] ADD CONSTRAINT [PK_dbo.QuestionDatas] PRIMARY KEY ([Id]);";

            using (IDbTransaction tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(createSql);

                await _db.ExecuteAsync(alterSql);

                tran.Commit();
            }
        }

        private async Task CreateTableP2Code()
        {
            var createSql =
                "CREATE TABLE [QuestionP2Code] ( \r\n" +
                "  [QuestionId] int NOT NULL     \r\n" +
                ", [Code] ntext NOT NULL         \r\n" +
                ", [Language] int NOT NULL       \r\n" +
                ", [RunTimes] smallint NOT NULL  \r\n" +
                ", [TimeLimitMs] int NOT NULL    \r\n" +
                ", [MemoryLimitMb] real NOT NULL \r\n" +
                ", [UpdateTicks] bigint NOT NULL \r\n" +
                ");                              \r\n";

            var alterSql =
                "ALTER TABLE [QuestionP2Code] ADD CONSTRAINT [PK_dbo.QuestionP2Code] PRIMARY KEY ([QuestionId]);";

            using (IDbTransaction tran = _db.BeginTransaction())
            {
                await _db.ExecuteAsync(createSql);

                await _db.ExecuteAsync(alterSql);

                tran.Commit();
            }
        }
    }
}