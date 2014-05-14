using SdojWeb.Infrastructure.Tasks;
using SdojWeb.Models;
using System.Data;
using System.Data.Entity;
using System.Web;

namespace SdojWeb.Infrastructure
{
    public class TransactionPerRequest :
        IRunOnEachRequest, IRunOnError, IRunAfterEachRequest
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
            _httpContext.Items["_Transaction"] =
                _dbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        void IRunOnError.Execute()
        {
            _httpContext.Items["_Error"] = true;
        }

        void IRunAfterEachRequest.Execute()
        {
            var transaction = _httpContext.Items["_Transaction"] as DbContextTransaction;

            if (transaction != null)
            {
                if (_httpContext.Items["_Error"] != null)
                {
                    transaction.Rollback();
                }
                else
                {
                    transaction.Commit();
                }
            }
        }
    }
}