using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Macros;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD.CBU.CaasDeploy.Library.Tests.Macros
{
    /// <summary>
    /// Unit tests for the <see cref="NextAvailablePublicIpMacro"/> class.
    /// </summary>
    [TestClass]
    public class NextAvailablePublicIpMacroTests
    {
        /// <summary>
        /// The org identifier.
        /// </summary>
        private const string OrgId = "68819F2B-22F9-4D46-86B8-7926B640464C";

        /// <summary>
        /// The network domain identifier.
        /// </summary>
        private const string NetworkDomainId = "EAA47216-8A92-4B3B-9277-67A8F46E8CB7";

        /// <summary>
        /// The runtime context.
        /// </summary>
        private readonly RuntimeContext _runtimeContext = new RuntimeContext
        {
            LogProvider = new ConsoleLogProvider(),
            AccountDetails = new CaasAccountDetails
            {
                BaseUrl = "https://api-au.dimensiondata.com",
                OrgId = OrgId
            }
        };

        /// <summary>
        /// The task context.
        /// </summary>
        private readonly TaskContext _taskContext = new TaskContext
        {
            Parameters = new Dictionary<string, string>
            {
                { "networkDomainId", NetworkDomainId }
            }
        };

        /// <summary>
        /// Tests the macro under normal conditions with an existing block.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task NextAvailablePublicIp_SubstituteTokensInString_ExistingBlock_Success()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/caas/2.0/" + OrgId + "/network/reservedPublicIpv4Address?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_ReservedPublicIpv4Address_Get.json");
            client.AddResponse("/caas/2.0/" + OrgId + "/network/publicIpBlock?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_PublicIpBlock_Get.json");

            var macro = new NextAvailablePublicIpMacro();
            var input = "$nextAvailablePublicIP[$parameters['networkDomainId']]";
            var output = await Macro.SubstituteTokensInString(_runtimeContext, _taskContext, input);

            Assert.AreEqual("168.128.52.11", output);
        }

        /// <summary>
        /// Tests the macro under normal conditions with a new block.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task NextAvailablePublicIp_SubstituteTokensInString_NewBlock_Success()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/caas/2.0/" + OrgId + "/network/reservedPublicIpv4Address?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_ReservedPublicIpv4Address_Get.json");
            client.AddResponse("/caas/2.0/" + OrgId + "/network/publicIpBlock?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_PublicIpBlock_Get_Empty.json");
            client.AddResponse("/caas/2.0/" + OrgId + "/network/addPublicIpBlock", "NextAvailablePublicIp_PublicIpBlock_Add.json");
            client.AddResponse("/caas/2.0/" + OrgId + "/network/reservedPublicIpv4Address?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_ReservedPublicIpv4Address_Get.json");
            client.AddResponse("/caas/2.0/" + OrgId + "/network/publicIpBlock?networkDomainId=" + NetworkDomainId, "NextAvailablePublicIp_PublicIpBlock_Get.json");

            var macro = new NextAvailablePublicIpMacro();
            var input = "$nextAvailablePublicIP[$parameters['networkDomainId']]";
            var output = await Macro.SubstituteTokensInString(_runtimeContext, _taskContext, input);

            Assert.AreEqual("168.128.52.11", output);
        }
    }
}
