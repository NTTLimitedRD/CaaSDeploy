using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DD.CBU.CaasDeploy.Library.Tests
{
    /// <summary>
    /// Integration tests for the <see cref="TemplateParser" /> class.
    /// </summary>
    [TestClass]
    public class TemplateParserTests
    {
        /// <summary>
        /// The path to the resources folder.
        /// </summary>
        private string _resourceFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Resources\Templates");

        /// <summary>
        /// The account details.
        /// </summary>
        private CaasAccountDetails _accountDetails = new CaasAccountDetails
        {
            BaseUrl = "https://api-na.dimensiondata.com",
            OrgId = "68819F2B-22F9-4D46-86B8-7926B640464C"
        };

        /// <summary>
        /// Tests the deployment process of a template.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task ParseAndExecuteDeploymentTasks_Success()
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
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(templateFile, parametersFile);
            var log = await taskExecutor.Execute(_accountDetails);

            Assert.AreEqual(DeploymentLogStatus.Success, log.Status);
        }

        /// <summary>
        /// Tests the deployment process of a template when an error occurs.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task ParseAndExecuteDeploymentTasks_Failed()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/network/networkDomain/d5791a6d-2b69-47e2-be06-f26a2ec4bff8", "NetworkDomain_Get.json");
            client.AddResponse("/network/vlan?name=Unit Test VLAN", "GenericNotFound.json");
            client.AddResponse("/network/deployVlan", "GenericError.json", HttpStatusCode.BadRequest);

            var templateFile = Path.Combine(_resourceFolder, "StandardTemplate.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(templateFile, parametersFile);
            var log = await taskExecutor.Execute(_accountDetails);

            Assert.AreEqual(DeploymentLogStatus.Failed, log.Status);
            Assert.AreEqual(1, log.Resources.Count);
            Assert.AreEqual(ResourceLogStatus.Failed, log.Resources[0].DeploymentStatus);
            Assert.AreEqual("UNEXPECTED_ERROR", log.Resources[0].Error.ResponseCode);
        }

        /// <summary>
        /// Tests the deletion process of a template.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task ParseAndExecuteDeletionTasks_Success()
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
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentLog(logFile);
            var log = await taskExecutor.Execute(_accountDetails);

            Assert.AreEqual(DeploymentLogStatus.Success, log.Status);
        }

        /// <summary>
        /// Tests the deletion process of a template when an error occurs.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task ParseAndExecuteDeletionTasks_Failed()
        {
            var client = new FakeHttpClient();
            client.AddResponse("/network/deleteNatRule", "NatRule_Delete.json");
            client.AddResponse("/network/natRule/a6b2e743-e330-4deb-a76e-0d9cb0b1d1bb", "NatRule_Get_NotFound.json", HttpStatusCode.BadRequest);
            client.AddResponse("/network/removePublicIpBlock", "GenericError.json", HttpStatusCode.BadRequest);

            var logFile = Path.Combine(_resourceFolder, "StandardTemplateLog.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentLog(logFile);
            var log = await taskExecutor.Execute(_accountDetails);

            Assert.AreEqual(DeploymentLogStatus.Failed, log.Status);
            Assert.AreEqual(2, log.Resources.Count);
            Assert.AreEqual(ResourceLogStatus.Deleted, log.Resources[0].DeploymentStatus);
            Assert.AreEqual(ResourceLogStatus.Failed, log.Resources[1].DeploymentStatus);
            Assert.AreEqual("UNEXPECTED_ERROR", log.Resources[1].Error.ResponseCode);
        }

        /// <summary>
        /// Tests that parsing a template throws an exception if a dependency is missing.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public void MissingDependency()
        {
            var templateFile = Path.Combine(_resourceFolder, "MissingDependency.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(templateFile, parametersFile);
        }

        /// <summary>
        /// Tests that parsing a template throws an exception if circular dependencies exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TemplateParserException))]
        public void CircularDependency()
        {
            var templateFile = Path.Combine(_resourceFolder, "CircularDependency.json");
            var parametersFile = Path.Combine(_resourceFolder, "StandardTemplateParams.json");
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(templateFile, parametersFile);
        }
    }
}
