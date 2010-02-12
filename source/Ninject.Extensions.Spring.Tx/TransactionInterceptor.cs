using System;
using Ninject.Extensions.Interception;
using Spring.Transaction;
using Spring.Transaction.Interceptor;

namespace Ninject.Extensions.Spring.Tx
{
    public class TransactionInterceptor : TransactionAspectSupport, ITransactionInterceptor
    {
        public TransactionInterceptor(
            ITransactionAttributeSource transactionAttributeSource, 
            IPlatformTransactionManager transactionManager)
        {
            TransactionAttributeSource = transactionAttributeSource;
            TransactionManager = transactionManager;
        }

        public void Intercept(IInvocation invocation)
        {
            Type targetType = invocation.Request.Target.GetType();

            TransactionInfo transactionInfo = CreateTransactionIfNecessary(invocation.Request.Method, targetType);
            
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                CompleteTransactionAfterThrowing(transactionInfo, ex);
                throw;
            }
            finally
            {
                CleanupTransactionInfo(transactionInfo);
            }
            CommitTransactionAfterReturning(transactionInfo);            
        }
    }
}