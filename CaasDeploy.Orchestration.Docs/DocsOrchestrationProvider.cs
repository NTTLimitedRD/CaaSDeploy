using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Orchestration.Docs
{
    /// <summary>
    /// Runs an orchestration using the R&D configuration managment and orchestration solution.
    /// </summary>
    /// <example>
    /// This provider requires an orchestration element in the template with the following structure.
    /// "orchestration": {
    ///   "provider": "CaasDeploy.Orchestration.Docs.DocsOrchestrationProvider, CaasDeploy.Orchestration.Docs",
    ///   "configuration": [
    ///     {
    ///       "scopePath": "/ROOT/foo",
    ///       "properties": {
    ///         "bar": "ConstantValue",
    ///         "baz": "$parameters['vmName']",
    ///         "spong": "$resources['MyVM'].id"
    ///       }
    ///     },
    ///     {
    ///       "scopePath": "/ROOT/foo/$parameters['vmName']",
    ///       "properties": {
    ///         "baz": "w00t"
    ///       }
    ///     }
    ///   ],
    ///   "environment": {
    ///     "customerCode": "FOO",
    ///     "environmentName": "Bar Environment",
    ///     "environmentScope": "foo",
    ///     "environmentDatacentre": "$resources['MyVM'].datacenterId",
    ///     "serverScopes": {
    ///       "MyVM": "$parameters['vmName']"
    ///     }
    ///   },
    ///   "runbook": "DeploySomeStuff"
    /// </example>
    public class DocsOrchestrationProvider : IOrchestrationProvider
    {
        /// <summary>
        /// The log provider.
        /// </summary>
        private ILogProvider _logProvider;

        /// <summary>
        /// The docs API client.
        /// </summary>
        private DocsApiClient _docsApiClient;

        /// <summary>
        /// The orchestrator API client.
        /// </summary>
        private OrchestratorApiClient _orchestratorClient;

        /// <summary>
        /// Runs the orchestration.
        /// </summary>
        /// <param name="orchestrationObject">The orchestration object.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task RunOrchestration(
            JObject orchestrationObject,
            IDictionary<string, string> parameters,
            IEnumerable<Resource> resources,
            IDictionary<string, JObject> resourcesProperties,
            ILogProvider logProvider)
        {
            _logProvider = logProvider;
            _docsApiClient = new DocsApiClient(orchestrationObject["docsServiceUrl"].Value<string>());
            _orchestratorClient = new OrchestratorApiClient(orchestrationObject["orchestratorServiceUrl"].Value<string>());

            TokenHelper.SubstituteTokensInJObject(orchestrationObject, parameters, resourcesProperties);

            await SendConfiguration((JArray)orchestrationObject["configuration"]);
            await SendEnvironment((JObject)orchestrationObject["environment"], resources, resourcesProperties);
            LaunchRunbook(orchestrationObject["runbook"].Value<string>());
        }

        /// <summary>
        /// Sends the configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task SendConfiguration(JArray configuration)
        {
            _logProvider.LogMessage("Sending configuration information to DOCS");
            foreach (var scope in configuration)
            {
                string scopePath = scope["scopePath"].Value<string>();
                await CreateScopes(scopePath);
                await AddProperties(scopePath.Split('/').Last(), (JObject)scope["properties"]);
            }
        }

        /// <summary>
        /// Creates the scopes.
        /// </summary>
        /// <param name="scopePath">The scope path.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
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

        /// <summary>
        /// Adds the properties.
        /// </summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task AddProperties(string scopeName, JObject properties)
        {
            await _docsApiClient.AddConfigProperties(scopeName, properties);
        }

        /// <summary>
        /// Sends the environment.
        /// </summary>
        /// <param name="environmentObject">The environment object.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task SendEnvironment(JObject environmentObject, IEnumerable<Resource> resources, IDictionary<string, JObject> resourcesProperties)
        {
            _logProvider.LogMessage("Sending environment information to DOCS");

            await CreateEnvironment(environmentObject);

            await CreateEnvironmentCredentials(
                environmentObject["environmentName"].Value<String>(),
                (JArray)environmentObject["credentials"]);

            await CreateServers(
                environmentObject["environmentName"].Value<String>(),
                (JObject)environmentObject["serverScopes"],
                resources,
                resourcesProperties);
        }

        /// <summary>
        /// Creates the environment.
        /// </summary>
        /// <param name="environmentObject">The environment object.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task CreateEnvironment(JObject environmentObject)
        {
            var customerCode = environmentObject["customerCode"].Value<string>();
            var customer = await _docsApiClient.GetCustomer(customerCode);
            var customerId = customer["Id"].Value<string>();

            var scopeName = environmentObject["environmentScope"].Value<string>();
            var scope = await _docsApiClient.GetScope(scopeName);
            var scopeId = scope["Id"].Value<string>();

            await _docsApiClient.AddEnvironment(
                environmentObject["environmentName"].Value<String>(),
                environmentObject["environmentDatacentre"].Value<String>(),
                Guid.Parse(customerId),
                Guid.Parse(scopeId));
        }

        /// <summary>
        /// Creates the environment credentials.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task CreateEnvironmentCredentials(string environmentName, JArray credentials)
        {
            foreach (var credential in credentials)
            {
                await _docsApiClient.AddCredential(
                    environmentName,
                    credential["name"].Value<string>(),
                    credential["type"].Value<string>(),
                    credential["userName"].Value<string>(),
                    credential["password"].Value<string>());
            }
        }

        /// <summary>
        /// Creates the servers.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="serverScopesObject">The server scopes object.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task CreateServers(string environmentName, JObject serverScopesObject, IEnumerable<Resource> resources, IDictionary<string, JObject> resourcesProperties)
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

        /// <summary>
        /// Launches the runbook.
        /// </summary>
        /// <param name="runbook">The runbook.</param>
        private void LaunchRunbook(string runbook)
        {
            _logProvider.LogMessage($"Launching runbook '{runbook}'");
            var jobId = _orchestratorClient.StartRunbookWithParameters(Guid.Parse(runbook), null);
            _logProvider.LogMessage($"Running... job id is {jobId}");
        }
    }
}
