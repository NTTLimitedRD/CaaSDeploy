using System;
using System.IO;
using System.Threading.Tasks;

using CaasDeploy.Library.Models;
using CaasDeploy.Library.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaasDeploy.Library.Tests
{
    [TestClass]
    public class TaskBuilderAndExecutorTests
    {
        [TestMethod]
        public async Task EndToEndDeployment()
        {
            var accountDetails = new CaasAccountDetails
            {
                BaseUrl = "https://api-na.dimensiondata.com",
                OrgId = "68819F2B-22F9-4D46-86B8-7926B640464C"
            };

            var client = new FakeHttpClient();
            client.AddResponse("/network/networkDomain/d5791a6d-2b69-47e2-be06-f26a2ec4bff8", "GetNetworkDomainResponse.json");
            client.AddResponse("/network/vlan?name=Unit Test VLAN", "NotFoundResponse.json");
            client.AddResponse("/network/deployVlan", "DeployVlanResponse.json");
            client.AddResponse("/network/vlan/997e2084-00b1-4d1d-96ce-099946679c6f", "GetVlanResponse.json");
            client.AddResponse("/server/server?name=Unit Test Server", "NotFoundResponse.json");
            client.AddResponse("/server/deployServer", "DeployServerResponse.json");
            client.AddResponse("/server/server/b42b40e1-351a-4df9-b726-2ccff01f2767", "GetServerResponse.json");
            client.AddResponse("/network/addPublicIpBlock", "AddPublicIpBlockResponse.json");
            client.AddResponse("/network/publicIpBlock/996b066e-bdce-11e4-8c14-b8ca3a5d9ef8", "GetPublicIpBlockResponse.json");
            client.AddResponse("/network/natRule?networkDomainId=d5791a6d-2b69-47e2-be06-f26a2ec4bff8&internalIp=10.0.0.8", "NotFoundResponse.json");
            client.AddResponse("/network/createNatRule", "CreateNatRuleResponse.json");
            client.AddResponse("/network/natRule/a6b2e743-e330-4deb-a76e-0d9cb0b1d1bb", "GetNatRuleResponse.json");

            var resourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources\Templates");
            var templateFile = Path.Combine(resourceFolder, "StandardTemplate.json");
            var parametersFile = Path.Combine(resourceFolder, "StandardTemplateParams.json");

            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), accountDetails);
            var taskExecutor = taskBuilder.GetDeploymentTasks(templateFile, parametersFile);
            var log = await taskExecutor.Execute();

            Assert.AreEqual(DeploymentLogStatus.Success, log.status);
        }
    }
}
