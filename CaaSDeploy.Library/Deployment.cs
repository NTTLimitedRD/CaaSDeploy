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
        private Regex _parameterRegex = new Regex("\\$parameters\\['([^']*)'\\]");
        private Regex _resourcePropertyRegex = new Regex("\\$resources\\['(.*)'\\]\\.(.*)");
        private string _scriptPath;
        private TraceListener _logWriter;

        public Deployment(TraceListener logWriter)
        {
            _logWriter = logWriter;
        }

        public async Task<DeploymentLog> Deploy(string templateId, Dictionary<string, string> parameters, CaasAccountDetails accountDetails)
        {
            _scriptPath = new FileInfo(templateId).DirectoryName;
            var template = TemplateParser.ParseTemplate(templateId);

            Dictionary<string, JObject> resourcesProperties = new Dictionary<string, JObject>();

            var sortedResources = ResourceDependencies.DependencySort(template.resources).Reverse();
            var log = new DeploymentLog()
            {
                deploymentTime = DateTime.Now,
                region = accountDetails.Region,
                templateName = template.metadata.templateName,
                parameters = parameters,
                resources = new List<ResourceLog>(),
            };

            foreach (var resource in sortedResources)
            {
                try
                {
                    SubstituteTokens(resource.resourceDefinition, parameters, resourcesProperties);
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType,  accountDetails, new ConsoleTraceListener());
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
            log.status = "Success";
            return log;
        }

        private void CopyAndRunScripts(Resource resource, JObject details, Dictionary<string, string> parameters)
        {
            _logWriter.WriteLine($"Running deployment scripts.");
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
                _logWriter.WriteLine("\tCopying file " + scriptFile.Name);
                scripting.UploadScript(scriptFile.FullName);
                scriptFile.Delete();
            }
            scriptDirectory.Delete();

            string deployScript = SubstituteTokensInCommandLine(resource.scripts.onDeploy, parameters);
            _logWriter.WriteLine("\tExecuting script " + deployScript);
            scripting.ExecuteScript(deployScript);

        }

        private string SubstituteTokensInCommandLine(string commandLine, Dictionary<string, string> parameters)
        {
            var paramsMatches = _parameterRegex.Matches(commandLine);
            string newCommandLine = commandLine;
            if (paramsMatches.Count > 0)
            {
                foreach (Match paramsMatch in paramsMatches)
                {
                    string newValue = parameters[paramsMatch.Groups[1].Value];
                    newCommandLine = newCommandLine.Replace(paramsMatch.Groups[0].Value, newValue);
                }

            }
            return newCommandLine;
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
                        var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, accountDetails, new ConsoleTraceListener());
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


        private void SubstituteTokens(JObject resourceDefinition, Dictionary<string, string> parameters, Dictionary<string, JObject> resourcesProperties)
        {
            foreach (var parameter in resourceDefinition)
            {
                if (parameter.Value is JObject)
                {
                    SubstituteTokens((JObject)parameter.Value, parameters, resourcesProperties);
                }
                else if (parameter.Value is JValue)
                {
                    string tokenValue = parameter.Value.Value<string>();
                    var paramsMatch = _parameterRegex.Match(tokenValue);
                    if (paramsMatch.Success)
                    {
                        string newValue = parameters[paramsMatch.Groups[1].Value];
                        parameter.Value.Replace(new JValue(newValue));
                    }

                    if (resourcesProperties != null)
                    {
                        var propsMatch = _resourcePropertyRegex.Match(tokenValue);
                        if (propsMatch.Success)
                        {
                            string resourceId = propsMatch.Groups[1].Value;
                            string property = propsMatch.Groups[2].Value;
                            var newValue = resourcesProperties[resourceId].SelectToken(property).Value<string>();
                            parameter.Value.Replace(new JValue(newValue));
                        }
                    }
                }
            }
        }
    }
}
