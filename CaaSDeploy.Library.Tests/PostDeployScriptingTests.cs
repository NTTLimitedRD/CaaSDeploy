using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaasDeploy.Library.Tests
{
    /// <summary>
    /// Summary description for PostDeployScriptingTests
    /// </summary>
    [TestClass]
    public class PostDeployScriptingTests
    {
        public PostDeployScriptingTests()
        {
            //
            // TODO: Add constructor logic here
            //
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

        [TestMethod]
        public void TestMethod1()
        {
            var scripting = new PostDeployScripting("168.128.4.59", "administrator", "Password@1", PostDeployScripting.OSType.Windows);
            scripting.UploadScript(@"c:\temp\test1.ps1");
            int exit = scripting.ExecuteScript("powershell.exe test1.ps1 zzz");
            Assert.AreEqual(0, exit);
        }
    }
}
