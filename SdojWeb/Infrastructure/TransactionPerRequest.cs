using System.Data;
using SdojWeb.Infrastructure.Tasks;
using SdojWeb.Models;
using System.Data.Entity;
using System.Web;

namespace SdojWeb.Infrastructure
{
    public sealed class TransactionPerRequest :
        IRunOnEachRequest, 
        IRunOnError, 
        IRunAfterEachRequest
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpContextBase _httpContext;

        public TransactionPerRequest(
            ApplicationDbContext dbContext,
            HttpContextBase httpContext)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
        }

        void IRunOnEachRequest.Execute()
        {
            _httpContext.Items[TransactionKey] =
                _dbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        void IRunOnError.Execute()
        {
            _httpContext.Items[TransactionError] = true;
        }

        void IRunAfterEachRequest.Execute()
        {
            var transaction = _httpContext.Items[TransactionKey] as DbContextTransaction;

            if (transaction != null)
            {
                if (_httpContext.Items[TransactionError] != null)
                {
                    transaction.Rollback();
                }
                else
                {
                    transaction.Commit();
                }
            }
        }

        public const string TransactionKey = "_Transaction";
        public const string TransactionError = "_Error";
    }
}