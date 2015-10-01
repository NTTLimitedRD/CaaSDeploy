using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using System.IO;
using CaasDeploy.Library.Models;
using System.IO.Compression;
using System.Diagnostics;

namespace CaasDeploy.Library
{
    public class Deployment
    {
        private string _scriptPath;
        private ILogProvider _logWriter;

        public Deployment(ILogProvider logWriter)
        {
            _logWriter = logWriter;
        }

        public async Task<DeploymentLog> Deploy(string templateId, Dictionary<string, string> parameters, CaasAccountDetails accountDetails)
        {
            _scriptPath = new FileInfo(templateId).DirectoryName;
            var template = TemplateParser.ParseTemplate(templateId);

            Dictionary<string, JObject> resourcesProperties = new Dictionary<string, JObject>();

            var log = new DeploymentLog()
            {
                deploymentTime = DateTime.Now,
                region = accountDetails.Region,
                templateName = template.metadata.templateName,
                parameters = parameters,
                resources = new List<ResourceLog>(),
            };

            var sortedResources = ResourceDependencies.DependencySort(template.resources).Reverse();
            foreach (var resource in sortedResources)
            {
                try
                {
                    TokenHelper.SubstituteTokensInJObject(resource.resourceDefinition, parameters, resourcesProperties);
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType,  accountDetails, _logWriter);
                    var resourceLog = await deployer.DeployAndWait(resource.resourceDefinition.ToString());
                    log.resources.Add(resourceLog);

                    if (resourceLog.deploymentStatus == ResourceLog.DeploymentStatusFailed)
                    {
                        log.status = "Failed";
                        return log;
                    }

                    resourcesProperties.Add(resource.resourceId, resourceLog.details);

                    if (resource.scripts != null && resource.resourceType == "Server")
                    {
                        CopyAndRunScripts(resource, resourceLog.details, parameters);
                    }

                }
                catch (Exception ex)
                {
                    log.status = "Failed";
                    return log;
                }
            }

            if (template.orchestration != null)
            {
                await RunOrchestration(template.orchestration, parameters, sortedResources, resourcesProperties);
            }


            log.status = "Success";
            return log;
        }

        private async Task RunOrchestration(JObject orchestration, Dictionary<string, string> parameters, IEnumerable<Resource> resources, Dictionary<string, JObject> resourcesProperties)
        {
            var providerTypeName = orchestration["provider"].Value<String>();
            var providerType = Type.GetType(providerTypeName);
            if (providerType == null)
            {
                _logWriter.LogError($"Unable to create Orchestration Provider of type {providerTypeName}.");
                return;
            }
            var provider = (IOrchestrationProvider)Activator.CreateInstance(providerType);
            _logWriter.LogMessage($"Running Orchestration Provider '{providerTypeName}'.");
            await provider.RunOrchestration(orchestration, parameters, resources, resourcesProperties, _logWriter);
        }

        private void CopyAndRunScripts(Resource resource, JObject details, Dictionary<string, string> parameters)
        {
            _logWriter.LogMessage($"Running deployment scripts.");
            string ipv6Address = details["networkInfo"]["primaryNic"]["ipv6"].Value<string>();
            string ipv6Unc = IPv6ToUnc(ipv6Address);
            PostDeployScripting.OSType osType = details["operatingSystem"]["family"].Value<string>() == "WINDOWS" ?
                PostDeployScripting.OSType.Windows : PostDeployScripting.OSType.Linux;

            string userName = osType == PostDeployScripting.OSType.Windows ? "administrator" : "root";
            string password = resource.resourceDefinition["administratorPassword"].Value<string>();

            var scripting = new PostDeployScripting(ipv6Unc, userName, password, osType);

            string scriptPath = UnzipScriptBundle(resource.scripts.bundleFile);
            var scriptDirectory = new DirectoryInfo(scriptPath);
            foreach (var scriptFile in scriptDirectory.EnumerateFiles())
            {
                _logWriter.LogMessage("\tCopying file " + scriptFile.Name);
                scripting.UploadScript(scriptFile.FullName);
                scriptFile.Delete();
            }
            scriptDirectory.Delete();

            string deployScript = TokenHelper.SubstitutePropertyTokensInString(resource.scripts.onDeploy, parameters);
            _logWriter.LogMessage("\tExecuting script " + deployScript);
            scripting.ExecuteScript(deployScript);

        }



        private string UnzipScriptBundle(string bundleFile)
        {
            var bundlePath = Path.Combine(_scriptPath, bundleFile);
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ZipFile.ExtractToDirectory(bundlePath, path);
            return path;
        }

        private string IPv6ToUnc(string ipv6Address)
        {
            return ipv6Address.Replace(':', '-').Replace('%', 's') + ".ipv6-literal.net";
        }


        public async Task<string> Delete(DeploymentLog log, CaasAccountDetails accountDetails)
        {
            try
            {
                var reversedResources = new List<ResourceLog>(log.resources);
                reversedResources.Reverse();

                foreach (var resource in reversedResources)
                {
                    if (resource.details != null)
                    {
                        var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, accountDetails, new ConsoleLogProvider());
                        var caasId = resource.details["id"].Value<string>();
                        await deployer.DeleteAndWait(caasId);
                    }
                }
                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
            
        }

        public string DeleteSync(DeploymentLog log, CaasAccountDetails accountDetails)
        {
            var task = Delete(log, accountDetails);
            task.Wait();
            return task.Result;
        }


       
    }
}
