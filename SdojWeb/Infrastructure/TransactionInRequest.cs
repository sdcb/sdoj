using System.Data;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Tasks;
using SdojWeb.Models;
using System.Data.Entity;
using System.Web;

namespace SdojWeb.Infrastructure
{
    public sealed class TransactionInRequest :
        IRunOnError, 
        IRunAfterEachRequest
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpContextBase _httpContext;

        public TransactionInRequest(
            ApplicationDbContext dbContext,
            HttpContextBase httpContext)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
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

        public static void EnsureTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var httpContext = HttpContext.Current;
            var transaction = httpContext.Items[TransactionKey] as DbContextTransaction;

            if (transaction == null)
            {
                var dbCotnext = DependencyResolver.Current.GetService<ApplicationDbContext>();
                httpContext.Items[TransactionKey] = dbCotnext.Database.BeginTransaction(isolationLevel);
            }
        }

        public const string TransactionKey = "_Transaction";
        public const string TransactionError = "_Error";
    }
}