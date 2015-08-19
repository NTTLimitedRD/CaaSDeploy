using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Tests
{
    [TestClass]
    public class ResourceDeployerTests
    {
        [TestMethod, Ignore]
        public async Task TestCanGetResourceAndParseJson()
        {
            var creds = await CaasAuthentication.Authenticate("thollander_eng", "P@ssw0rd1!", "NA");
            var deployer = new ResourceDeployer("Foo", "Server", "NA", creds);
            var serverId = await deployer.GetResourceIdByName("TomsVM");
            var serverProps = await deployer.Get(serverId);
            var ip = serverProps.SelectToken("networkInfo.primaryNic.privateIpv4");
            Assert.AreEqual("10.0.0.11", ip);
        }
    }
}
