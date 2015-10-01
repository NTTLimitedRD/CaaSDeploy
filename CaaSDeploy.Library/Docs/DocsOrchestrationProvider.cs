using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CaasDeploy.Library.Utilities;
using System.Net.Http;

namespace CaasDeploy.Library.Docs
{
    public class DocsOrchestrationProvider : IOrchestrationProvider
    {
        private DocsApiClient _docsApiClient = new DocsApiClient();

        public async Task RunOrchestration(JObject orchestrationObject, Dictionary<string, string> parameters, Dictionary<string, JObject> resourcesProperties, ILogProvider logProvider)
        {
            TokenHelper.SubstituteTokensInJObject(orchestrationObject, parameters, resourcesProperties);

            await SendConfiguration((JArray)orchestrationObject["configuration"]);
        }

        private async Task SendConfiguration(JArray configuration)
        {
            foreach (var scope in configuration)
            {
                await CreateScopes(scope["scope"].Value<string>());
                //await CreateProperty(property.Name, property.Value.Value<string>());
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

        private async Task CreateProperty(string name, string v)
        {
            throw new NotImplementedException();
        }

       
    }
}
