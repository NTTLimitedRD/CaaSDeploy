using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Orchestration.Docs
{
    /// <summary>
    /// The docs API client.
    /// </summary>
    internal class DocsApiClient
    {
        /// <summary>
        /// The base URL.
        /// </summary>
        private readonly string _baseUrl;

        /// <summary>
        /// The HTTP client instance.
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocsApiClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public DocsApiClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            _client = new HttpClient(handler);
        }

        /// <summary>
        /// Calls the service.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="urlPath">The URL path.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="ensureSuccessCode">if set to <c>true</c> [ensure success code].</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task<string> CallService(HttpMethod method, string urlPath, string payload = null, bool ensureSuccessCode = true)
        {
            var request = new HttpRequestMessage(method, _baseUrl + urlPath);
            if (payload != null)
            {
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync(); 
            if (ensureSuccessCode)
            {
                response.EnsureSuccessStatusCode();
            }
            return responseString;
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task<JObject> GetScope(string scopeName)
        {
            var response = await CallService(HttpMethod.Get, $"api/Scope/{Uri.EscapeDataString(scopeName)}", null, false);
            if (String.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            return JObject.Parse(response);
        }

        /// <summary>
        /// Adds the scope.
        /// </summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <param name="parentScopeId">The parent scope identifier.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task<JObject> AddScope(string scopeName, string parentScopeId)
        {
            var payload = "{ \"Name\": \"" + scopeName + "\", ";
            if (parentScopeId != null)
            {
                payload += "\"ParentScopeId\": \"" + parentScopeId + "\", \"IsRoot\": false }";
            }
            else
            {
                payload += "\"IsRoot\": true }";
            }

            var respsonse = await CallService(HttpMethod.Post, "api/scope", payload);
            return JObject.Parse(respsonse);
        }

        /// <summary>
        /// Adds the configuration properties.
        /// </summary>
        /// <param name="scopeName">Name of the scope.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task AddConfigProperties(string scopeName, JObject properties)
        {
            var response = await CallService(HttpMethod.Post, $"api/Scope/{scopeName}/ConfigValues", properties.ToString());
        }

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task<JObject> GetCustomer(string code)
        {
            var response = await CallService(HttpMethod.Get, $"api/customer/{Uri.EscapeDataString(code)}", null, false);
            if (String.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            return JObject.Parse(response);
        }

        /// <summary>
        /// Adds the environment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataCentre">The data centre.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="configScopeId">The configuration scope identifier.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task AddEnvironment(string name, string dataCentre, Guid customerId, Guid configScopeId)
        {
            var payloadObject = new
            {
                Name = name,
                Datacentre = dataCentre,
                CustomerId = customerId,
                ConfigScopeId = configScopeId,
            };
            var payload = JsonConvert.SerializeObject(payloadObject);
            var response = await CallService(HttpMethod.Post, "api/Environment", payload);
        }

        /// <summary>
        /// Adds the server.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="adminUser">The admin user.</param>
        /// <param name="adminPassword">The admin password.</param>
        /// <param name="scopeId">The scope identifier.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task AddServer(string environmentName, string serverName, string ipAddress, string adminUser, string adminPassword, Guid scopeId)
        {
            var payloadObject = new
            {
                Name = serverName,
                IpAddress = ipAddress,
                Username = adminUser,
                Password = adminPassword,
                ConfigScopeId = scopeId,
            };
            var payload = JsonConvert.SerializeObject(payloadObject);
            var response = await CallService(HttpMethod.Post, $"api/Environment/{environmentName}/Server?encrypt=false", payload);
        }

        /// <summary>
        /// Adds the credential.
        /// </summary>
        /// <param name="environmentName">Name of the environment.</param>
        /// <param name="credentialName">Name of the credential.</param>
        /// <param name="credentialType">Type of the credential.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task AddCredential(string environmentName, string credentialName, string credentialType, string userName, string password)
        {
            var payloadObject = new
            {
                Name = credentialName,
                CredentialType = credentialType,
                Username = credentialType,
                Password = password,
            };
            var payload = JsonConvert.SerializeObject(payloadObject);
            var response = await CallService(HttpMethod.Post, $"api/Environment/{environmentName}/Credential?encrypt=false", payload);
        }
    }
}
