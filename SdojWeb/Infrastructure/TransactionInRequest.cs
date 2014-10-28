using System.Data;
using System.Web.Mvc;
using SdojWeb.Infrastructure.Tasks;
using SdojWeb.Models;
using System.Data.Entity;
using System.Web;

namespace SdojWeb.Infrastructure
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class TransactionInRequest :
        IRunOnError, 
        IRunAfterEachRequest
    {
        private readonly HttpContextBase _httpContext;

        public TransactionInRequest(
            HttpContextBase httpContext)
        {
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

        private const string TransactionKey = "_Transaction";
        private const string TransactionError = "_Error";
    }
}