using System.Configuration;
using System.Linq;

using DD.CBU.CaasDeploy.Library.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD.CBU.CaasDeploy.Library.Tests.Config
{
    /// <summary>
    /// Unit tests for the <see cref="ComputeConfigurationSection"/> class.
    /// </summary>
    [TestClass]
    public class ComputeConfigurationSectionTests
    {
        /// <summary>
        /// Tests loading of the configuration section.
        /// </summary>
        [TestMethod]
        public void LoadConfiguration()
        {
            var config = (ComputeConfigurationSection)ConfigurationManager.GetSection("compute");

            Assert.AreEqual(3, config.GetRegions().Count());
        }
    }
}
