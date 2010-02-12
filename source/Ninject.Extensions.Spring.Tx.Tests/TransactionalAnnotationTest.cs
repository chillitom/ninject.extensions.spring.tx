using Ninject.Extensions.Interception;
using Ninject.Modules;
using NUnit.Framework;

namespace Ninject.Extensions.Spring.Tx.Tests
{
    public class TestTransactionInterceptor : ITransactionInterceptor
    {
        public bool Invoked { get; private set; }

        public void Intercept(IInvocation invocation)
        {
            Invoked = true;
        }
    }

    public interface ITargetService
    {
        void TransactionalMethod();
        void NonTransactionalMethod();
    }

    public class TargetService : ITargetService
    {
        [Transaction]
        public virtual void TransactionalMethod() { }

        public virtual void NonTransactionalMethod() { }
    }

    class TransactionalAnnotationTestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITransactionInterceptor>().To<TestTransactionInterceptor>().InSingletonScope();
            Bind<ITargetService>().To<TargetService>();
        }
    }

    /// <summary>
    /// Tests that the Ninject.Extensions.Interception plug-in picks up our annotation correctly
    /// </summary>
    [TestFixture]
    public class TransactionalAnnotationTest
    {
        private ITargetService _targetService;
        private TestTransactionInterceptor _interceptor;

        [SetUp]
        public void SetUp()
        {
            IKernel kernel = new StandardKernel(new NinjectSettings {LoadExtensions = false});
            kernel.Load(new LinFuModule());
            kernel.Load(new TransactionalAnnotationTestModule());

            _targetService = kernel.Get<ITargetService>();
            _interceptor = (TestTransactionInterceptor) kernel.Get<ITransactionInterceptor>();
        }

        [Test]
        public void CheckInterceptorIsCalledOnAnnotatedMethod()
        {
            _targetService.TransactionalMethod();
            Assert.IsTrue(_interceptor.Invoked);
        }

        [Test]
        public void CheckInterceptorIsNotCalledOnNonAnnotatedMethod()
        {
            _targetService.NonTransactionalMethod();
            Assert.IsFalse(_interceptor.Invoked);
        }
 
    }
}