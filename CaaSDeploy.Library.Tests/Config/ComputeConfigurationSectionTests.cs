using System.Configuration;
using System.Linq;

using CaasDeploy.Library.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaasDeploy.Library.Tests.Config
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
