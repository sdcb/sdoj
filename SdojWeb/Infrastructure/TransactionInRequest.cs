using System.Transactions;
using System;

namespace SdojWeb.Infrastructure
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class TransactionInRequest
    {
        public static TransactionScope BeginTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5.0), TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}