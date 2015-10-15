using System.Configuration;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CaasDeploy.Library.Contracts;

namespace CaasDeploy.Library.Tests.Config
{
    [TestClass]
    public class ComputeConfigurationSectionTests
    {
        [TestMethod]
        public void LoadConfiguration()
        {
            var config = (IComputeConfiguration)ConfigurationManager.GetSection("compute");

            Assert.AreEqual(3, config.GetRegions().Count());
        }
    }
}
