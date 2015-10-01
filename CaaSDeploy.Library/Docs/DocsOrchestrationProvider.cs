using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CaasDeploy.Library.Utilities;
using System.Net.Http;
using CaasDeploy.Library.Models;

namespace CaasDeploy.Library.Docs
{
    public class DocsOrchestrationProvider : IOrchestrationProvider
    {
        private DocsApiClient _docsApiClient = new DocsApiClient();

        public async Task RunOrchestration(JObject orchestrationObject, Dictionary<string, string> parameters, IEnumerable<Resource> resources, 
            Dictionary<string, JObject> resourcesProperties, ILogProvider logProvider)
        {
            TokenHelper.SubstituteTokensInJObject(orchestrationObject, parameters, resourcesProperties);

            await SendConfiguration((JArray)orchestrationObject["configuration"]);
            await SendEnvironment((JObject)orchestrationObject["environment"], resources, resourcesProperties);
        }


        private async Task SendConfiguration(JArray configuration)
        {
            foreach (var scope in configuration)
            {
                string scopePath = scope["scopePath"].Value<string>();
                await CreateScopes(scopePath);
                await AddProperties(scopePath.Split('/').Last(), (JObject) scope["properties"]);
            }
        }

        private async Task CreateScopes(string scopePath)
        {
            var parts = scopePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string parentScopeId = null;
            foreach (var scopeName in parts)
            {
                var scope = await _docsApiClient.GetScope(scopeName);
                if (scope == null)
                {
                    var newScope = await _docsApiClient.AddScope(scopeName, parentScopeId);
                    parentScopeId = newScope["Id"].Value<string>();
                }
                else
                {
                    parentScopeId = scope["Id"].Value<string>();
                }

            }
            
        }

        private async Task AddProperties(string scopeName, JObject properties)
        {
            await _docsApiClient.AddConfigProperties(scopeName, properties);
        }

        private async Task SendEnvironment(JObject environmentObject, IEnumerable<Resource> resources, Dictionary<string, JObject> resourcesProperties)
        {
            await CreateEnvironment(environmentObject);
            await CreateServers(environmentObject["environmentName"].Value<String>(), 
                (JObject) environmentObject["serverScopes"], resources, resourcesProperties);
        }

        private async Task CreateEnvironment(JObject environmentObject)
        {
            var customerCode = environmentObject["customerCode"].Value<string>();
            var customer = await _docsApiClient.GetCustomer(customerCode);
            var customerId = customer["Id"].Value<string>();

            var scopeName =  environmentObject["environmentScope"].Value<string>();
            var scope = await _docsApiClient.GetScope(scopeName);
            var scopeId = scope["Id"].Value<string>();

            await _docsApiClient.AddEnvironment(environmentObject["environmentName"].Value<String>(),
                environmentObject["environmentDatacentre"].Value<String>(),
                Guid.Parse(customerId), Guid.Parse(scopeId));
        }

        private async Task CreateServers(string environmentName, JObject serverScopesObject, IEnumerable<Resource> resources, Dictionary<string, JObject> resourcesProperties)
        {
            foreach (var prop in serverScopesObject.Properties())
            {
                string serverName = resourcesProperties[prop.Name]["name"].Value<string>();
                string ipAddress = resourcesProperties[prop.Name]["networkInfo"]["primaryNic"]["privateIpv4"].Value<string>();
                string osFamily = resourcesProperties[prop.Name]["operatingSystem"]["family"].Value<string>();
                string adminUser = osFamily.ToLower() == "windows" ? "administrator" : "root";
                string adminPassword = resources.Where(r => r.resourceId == prop.Name).Single().resourceDefinition["administratorPassword"].Value<string>();

                var scopeName = prop.Value.Value<string>();
                var scope = await _docsApiClient.GetScope(scopeName);
                var scopeId = scope["Id"].Value<string>();
                await _docsApiClient.AddServer(environmentName, serverName, ipAddress, adminUser, adminPassword, Guid.Parse(scopeId));
            }
        }
    }
}
