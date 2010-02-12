using System;
using System.Data;
using Ninject.Extensions.Interception;
using NUnit.Framework;
using Spring.Data.Common;
using Spring.Data.Core;

namespace Ninject.Extensions.Spring.Tx.Tests
{
    public class Dao : AdoDaoSupport
    {
        public Dao(IDbProvider provider)
        {
            DbProvider = provider;
        }

        public void Insert(string s)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text, "INSERT INTO TransactionIntegrationTest(name) VALUES ('" + s + "')");
        }
    }

    public class Service
    {
        private readonly Dao _dao;

        public Service(Dao dao)
        {
            _dao = dao;
        }

        [Transaction]
        public virtual void CallBothDaosInSuccessfulTransaction()
        {
            _dao.Insert("A");
            _dao.Insert("B");
        }

        [Transaction]
        public virtual void CallBothDaosThenFailTransaction()
        {
            _dao.Insert("A");
            _dao.Insert("B");
            throw new ExpectedTestException();
        }
    }

    public class ExpectedTestException : Exception { }

    [TestFixture]
    [Category("Integration")]
    public class TransactionIntegrationTest
    {
        private Service _service;
        private AdoTemplate _adoTemplate;

        [SetUp]
        public void SetUp()
        {
            IKernel kernel = new StandardKernel(new NinjectSettings { LoadExtensions = false });
            kernel.Load(new LinFuModule());
            kernel.Load(new TransactionsModule());
            

            // TODO externalise the test database connection information
            IDbProvider dbProvider = DbProviderFactory.GetDbProvider("MySql.Data.MySqlClient");
            dbProvider.ConnectionString = "SERVER=localhost; DATABASE=chameleon; UID=root; PASSWORD=;";

            kernel.Bind<IDbProvider>().ToConstant(dbProvider).InSingletonScope();

            _adoTemplate = kernel.Get<AdoTemplate>();
            _service = kernel.Get<Service>();

            _adoTemplate.ExecuteNonQuery(CommandType.Text,
                                         "CREATE TABLE IF NOT EXISTS TransactionIntegrationTest ("
                                         + "   id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, "
                                         + "   `name` VARCHAR(100)" + ");"
                                         + "TRUNCATE TABLE TransactionIntegrationTest;");
        }

        [TearDown]
        public void TearDown()
        {
            _adoTemplate.ExecuteNonQuery(CommandType.Text, "DROP TABLE IF EXISTS TransactionIntegrationTest;");
        }

        private int Count(string s)
        {
            return Convert.ToInt32(_adoTemplate.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) FROM TransactionIntegrationTest WHERE name = '" + s + "'"));
        }

        [Test]
        public void SuccessfulSystemTransactionOperation()
        {
            _service.CallBothDaosInSuccessfulTransaction();
            Assert.AreEqual(1, Count("A"));
            Assert.AreEqual(1, Count("B"));
        }


        /// n.b. if this test fails check that the default table type for the DB is transaction capable (i.e. InnoDB)
        [Test]
        [ExpectedException(typeof(ExpectedTestException))]
        public void UnsuccessfulSystemTransactionOperation()
        {
            try
            {
                _service.CallBothDaosThenFailTransaction();                
            } 
            catch (Exception)
            {
                Assert.AreEqual(0, Count("A"));
                Assert.AreEqual(0, Count("B"));
                throw;
            }
        }

    }
}