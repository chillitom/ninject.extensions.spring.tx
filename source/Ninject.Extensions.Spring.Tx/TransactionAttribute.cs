using System;
using System.Transactions;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Attributes;
using Ninject.Extensions.Interception.Request;
using Spring.Transaction;
using Spring.Transaction.Support;
using IsolationLevel=System.Data.IsolationLevel;

namespace Ninject.Extensions.Spring.Tx
{
    public class TransactionAttribute : InterceptAttribute
    {
        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {        
            return request.Kernel.Get<ITransactionInterceptor>();
        }

        private readonly TransactionPropagation _transactionPropagation = TransactionPropagation.Required;
        private readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        private int _timeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;
        private Type[] _rollbackTypes = Type.EmptyTypes;
        private Type[] _noRollbackTypes = Type.EmptyTypes;
        private EnterpriseServicesInteropOption _esInteropOption = EnterpriseServicesInteropOption.Automatic;

        public TransactionAttribute()
        {
        }

        /// <param name="transactionPropagation">The transaction propagation.</param>
        public TransactionAttribute(TransactionPropagation transactionPropagation) : this()
        {
            _transactionPropagation = transactionPropagation;
        }

        /// <param name="transactionPropagation">The transaction propagation.</param>
        /// <param name="isolationLevel">The isolation level.</param>
        public TransactionAttribute(TransactionPropagation transactionPropagation,
                                    IsolationLevel isolationLevel) : this(transactionPropagation)
        {
            _isolationLevel = isolationLevel;
        }

        /// <param name="isolationLevel">The isolation level.</param>
        public TransactionAttribute(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }


        /// <remarks>Defaults to TransactionPropagation.Required</remarks>
        /// <value>The transaction propagation.</value>
        public TransactionPropagation TransactionPropagation
        {
            get { return _transactionPropagation; }
        }

        /// <remarks>Defaults to IsolationLevel.Unspecified</remarks>
        /// <value>The isolation level.</value>
        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }

        /// <remarks>Defaults to the default timeout of the underlying transaction system.</remarks>
        /// <value>The timeout.</value>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <remarks>Defaults to false</remarks>
        /// <value><c>true</c> if read-only; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// exception types which must cause a transaction rollback.
        /// </summary>
        public Type[] RollbackFor
        {
            get { return _rollbackTypes; }
            set { _rollbackTypes = value; }
        }

        /// <summary>
        /// exceptions types which must <c>not</c> cause a transaction rollback.
        /// </summary>
        public Type[] NoRollbackFor
        {
            get { return _noRollbackTypes; }
            set { _noRollbackTypes = value; }
        }
       
        /// <value>The enterprise services interop option.</value>
        public EnterpriseServicesInteropOption EnterpriseServicesInteropOption
        {
            get { return _esInteropOption;}
            set { _esInteropOption = value; }
        }

    }
}