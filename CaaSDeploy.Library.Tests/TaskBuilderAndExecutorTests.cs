using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using CaasDeploy.Library.Models;
using CaasDeploy.Library.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaasDeploy.Library.Tests
{
    [TestClass]
    public class TaskBuilderAndExecutorTests
    {
        private string _resourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources\Templates");

        private CaasAccountDetails _accountDetails = new CaasAccountDetails
        {
            BaseUrl = "https://api-na.dimensiondata.com",
            OrgId = "68819F2B-22F9-4D46-86B8-7926B640464C"
        };

        [TestMethod]
        public async Task GetAndExecuteDeploymentTasks()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/network/networkDomain/d5791a6d-2b69-47e2-be06-f26a2ec4bff8", "NetworkDomain_Get.json");
            client.AddResponse("/network/vlan?name=Unit Test VLAN", "GenericNotFound.json");
            client.AddResponse("/network/deployVlan", "Vlan_Delpoy.json");
            client.AddResponse("/network/vlan/997e2084-00b1-4d1d-96ce-099946679c6f", "Vlan_Get.json");
            client.AddResponse("/server/server?name=Unit Test Server", "GenericNotFound.json");
            client.AddResponse("/server/deployServer", "Server_Deploy.json");
            client.AddResponse("/server/server/b42b40e1-351a-4df9-b726-2ccff01f2767", "Server_Get.json");
            client.AddResponse("/network/addPublicIpBlock", "PublicIpBlock_Add.json");
            client.AddResponse("/network/publicIpBlock/996b066e-bdce-11e4-8c14-b8ca3a5d9ef8", "PublicIpBlock_Get.json");
            client.AddResponse("/network/natRule?networkDomainId=d5791a6d-2b69-47e2-be06-f26a2ec4bff8&internalIp=10.0.0.8", "GenericNotFound.json");
            client.AddResponse("/network/createNatRule", "NatRule_Create.json");
            client.AddResponse("/network/natRule/a6b2e743-e330-4deb-a76e-0d9cb0b1d1bb", "NatRule_Get.json");

            var templateFile = Path.Combine(_resourceFolder, "StandardTemplate.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), _accountDetails);
            var taskExecutor = taskBuilder.GetDeploymentTasks(templateFile, parametersFile);
            var log = await taskExecutor.Execute();

            Assert.AreEqual(DeploymentLogStatus.Success, log.status);
        }

        [TestMethod]
        public async Task GetAndExecuteDeletionTasks()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/network/deleteNatRule", "NatRule_Delete.json");
            client.AddResponse("/network/natRule/a6b2e743-e330-4deb-a76e-0d9cb0b1d1bb", "NatRule_Get_NotFound.json", HttpStatusCode.BadRequest);
            client.AddResponse("/network/removePublicIpBlock", "PublicIpBlock_Delete.json");
            client.AddResponse("/network/publicIpBlock/996b066e-bdce-11e4-8c14-b8ca3a5d9ef8", "PublicIpBlock_Get_NotFound.json", HttpStatusCode.BadRequest);
            client.AddResponse("/server/deleteServer", "Server_Delete.json");
            client.AddResponse("/server/server/b42b40e1-351a-4df9-b726-2ccff01f2767", "Server_Get_NotFound.json", HttpStatusCode.BadRequest);
            client.AddResponse("/network/deleteVlan", "Vlan_Delete.json");
            client.AddResponse("/network/vlan/997e2084-00b1-4d1d-96ce-099946679c6f", "Vlan_Get_NotFound.json", HttpStatusCode.BadRequest);

            var logFile = Path.Combine(_resourceFolder, "StandardTemplateLog.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), _accountDetails);
            var taskExecutor = taskBuilder.GetDeletionTasks(logFile);
            var log = await taskExecutor.Execute();

            Assert.AreEqual(DeploymentLogStatus.Success, log.status);
        }

        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public void MissingDependency()
        {
            var templateFile = Path.Combine(_resourceFolder, "MissingDependency.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), _accountDetails);
            var taskExecutor = taskBuilder.GetDeploymentTasks(templateFile, parametersFile);
        }

        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public void CircularDependency()
        {
            var templateFile = Path.Combine(_resourceFolder, "CircularDependency.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), _accountDetails);
            var taskExecutor = taskBuilder.GetDeploymentTasks(templateFile, parametersFile);
        }
    }
}
