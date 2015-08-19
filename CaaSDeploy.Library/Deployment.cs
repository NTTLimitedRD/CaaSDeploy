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

namespace CaasDeploy.Library
{
    public class Deployment
    {
        private string _region;
        private CaasAccountDetails _accountDetails;
        private Regex _parameterRegex = new Regex("\\$parameters\\['(.*)'\\]");
        private Regex _resourcePropertyRegex = new Regex("\\$resources\\['(.*)'\\]\\.(.*)");

        public Deployment(string region, CaasAccountDetails accountDetails)
        {
            _region = region;
            _accountDetails = accountDetails;
        }

        public async Task Deploy(string templateFile, string parametersFile, string logFile)
        {
            await Deploy(templateFile, TemplateParser.ParseParameters(parametersFile), logFile);
        }

        public async Task Deploy(string templateFile, Dictionary<string, string> parameters, string logFile)
        {
            var template = TemplateParser.ParseTemplate(templateFile);
            Dictionary<string, JObject> resourcesProperties = new Dictionary<string, JObject>();

            var sortedResources = ResourceDependencies.DependencySort(template.resources).Reverse();
            var log = new DeploymentLog()
            {
                deploymentTime = DateTime.Now,
                region = _region,
                templateFile = templateFile,
                parameters = parameters,
                resources = new List<ResourceLog>(),
            };

            foreach (var resource in sortedResources)
            {
                try
                {
                    SubstituteTokens(resource.resourceDefinition, parameters, resourcesProperties);
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, _region, _accountDetails);
                    var properties = await deployer.DeployAndWait(resource.resourceDefinition.ToString());

                    resourcesProperties.Add(resource.resourceId, properties);

                    log.resources.Add(new ResourceLog()
                    {
                        resourceId = resource.resourceId,
                        resourceType = resource.resourceType,
                        details = properties,
                    });
                }
                catch (CaasException ex)
                {
                    log.resources.Add(new ResourceLog()
                    {
                        resourceId = resource.resourceId,
                        resourceType = resource.resourceType,
                        error = ex.FullResponse,
                    });
                    WriteLog(log, logFile);
                    throw;
                }
            }
            WriteLog(log, logFile);
        }

        private void WriteLog(DeploymentLog log, string logFile)
        {
            using (var sw = new StreamWriter(logFile))
            {
                var json = JsonConvert.SerializeObject(log, Formatting.Indented);
                sw.Write(json);
            }
        }

        public async Task Delete(string logFile)
        {
            var log = TemplateParser.ParseDeploymentLog(logFile);

            var reversedResources = new List<ResourceLog>(log.resources);
            reversedResources.Reverse();

            foreach (var resource in reversedResources)
            {
                if (resource.details != null)
                {
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, _region, _accountDetails);
                    var caasId = resource.details["id"].Value<string>();
                    await deployer.DeleteAndWait(caasId);
                }
            }
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
