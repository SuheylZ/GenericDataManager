using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericDataManager.Interfaces;
using GenericDataManager;
using System.Configuration;
using GenericDataManager.Common;

namespace UnitTests
{
    /// <summary>
    /// Summary description for DataManagerTestBase
    /// </summary>
    [TestClass]
    public class TestBase
    {
        protected IDataRepositoryProvider manager;

        public TestBase()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var arg = new ConnectionParameters(
               ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString,
               ConfigurationManager.AppSettings["model"],
               10);

            var policy = new ExecutionPolicy
            {
                PeriodicDisposalStrategy = Strategy.DisposeWhenNotInUse,
                FinalDisposalBehaviour = ManagerDisposalStrategy.DisposeButThrowIfInUse, 
                HeartBeat = TimeSpan.FromSeconds(30)
            };

            manager = new DataManagerWithPolicy(arg, policy);
        }

        [TestCleanup]
        public void CleanUp()
        {
            manager.Dispose();
        }
    }
}
