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

namespace CaasDeploy.Library
{
    public class Deployment
    {
        private Regex _parameterRegex = new Regex("\\$parameters\\['(.*)'\\]");
        private Regex _resourcePropertyRegex = new Regex("\\$resources\\['(.*)'\\]\\.(.*)");

        public Deployment()
        {
            
        }

        public async Task<DeploymentLog> Deploy(DeploymentTemplate template, Dictionary<string, string> parameters, CaasAccountDetails accountDetails)
        {
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
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType,  accountDetails);
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
                    log.status = "Failed";
                    return log;
                }
            }
            log.status = "Success";
            return log;
        }


        public DeploymentLog DeploySync(DeploymentTemplate template, Dictionary<string, string> parameters, CaasAccountDetails accountDetails)
        {
            var task = Deploy(template, parameters, accountDetails);
            task.Wait();
            return task.Result;
        }


        public async Task Delete(DeploymentLog log, CaasAccountDetails accountDetails)
        {
            var reversedResources = new List<ResourceLog>(log.resources);
            reversedResources.Reverse();

            foreach (var resource in reversedResources)
            {
                if (resource.details != null)
                {
                    var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, accountDetails);
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
