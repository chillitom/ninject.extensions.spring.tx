using Ninject.Modules;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Transaction.Interceptor;

namespace Ninject.Extensions.Spring.Tx
{
    public class TransactionsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITransactionInterceptor>().To<TransactionInterceptor>().InSingletonScope();
            Bind<ITransactionAttributeSource>().To<TransactionAttributeSource>().InSingletonScope();
            Bind<IPlatformTransactionManager>().To<TxScopeTransactionManager>().InSingletonScope();
        }
    }
}