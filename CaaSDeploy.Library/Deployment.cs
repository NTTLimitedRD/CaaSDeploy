using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace CaasDeploy.Library
{
    public class Deployment
    {
        private DeploymentTemplate _template;
        private Dictionary<string, string> _parameters;
        private string _region;
        private CaasAccountDetails _accountDetails;
        private Regex _parameterRegex = new Regex("\\$parameters\\['(.*)'\\]");
        private Regex _resourcePropertyRegex = new Regex("\\$resources\\['(.*)'\\]\\.(.*)");

        public Deployment(string templateFile, string parametersFile, string region, CaasAccountDetails accountDetails)
            : this(templateFile, region, accountDetails)
        {
            _parameters = TemplateParser.ParseParameters(parametersFile);
        }

        public Deployment(string templateFile, Dictionary<string, string> parameters, string region, CaasAccountDetails accountDetails)
             : this(templateFile, region, accountDetails)
        {
            _parameters = parameters;
        }

        public Deployment(string templateFile, string region, CaasAccountDetails accountDetails)
        {
            _template = TemplateParser.ParseTemplate(templateFile);
            _region = region;
            _accountDetails = accountDetails;
        }

        public async Task Deploy()
        {
            Dictionary<string, Dictionary<string, string>> resourcesProperties = new Dictionary<string, Dictionary<string, string>>();

            foreach (var resource in _template.resources)
            {
                // TODO: Sort the resources by dependency
                SubstituteTokens(resource.resourceDefinition, _parameters, resourcesProperties);
                var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, _region, _accountDetails);
                var properties = await deployer.DeployAndWait(resource.resourceDefinition.ToString());
                resourcesProperties.Add(resource.resourceId, properties);
            }
        }

        public async Task Delete()
        {
            var reversedResources = new List<Resource>(_template.resources);
            reversedResources.Reverse();
            // TODO: Sort the resources by dependency
            foreach (var resource in reversedResources)
            {
                SubstituteTokens(resource.resourceDefinition, _parameters, null);
                var deployer = new ResourceDeployer(resource.resourceId, resource.resourceType, _region, _accountDetails);
                var id = await deployer.GetResourceIdByName(resource.resourceDefinition["name"].Value<string>());
                if (id != null)
                {
                    await deployer.DeleteAndWait(id);
                }
            }
        }


        private void SubstituteTokens(JObject resourceDefinition, Dictionary<string, string> parameters, Dictionary<string, Dictionary<string, string>> resourcesProperties)
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
                            var newValue = resourcesProperties[resourceId][property];
                            parameter.Value.Replace(new JValue(newValue));
                        }
                    }
                }
            }
        }
    }
}
