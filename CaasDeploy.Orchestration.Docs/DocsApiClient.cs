using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Orchestration.Docs
{
    class DocsApiClient
    {
        private const string _baseUrl = "http://localhost:52119/";
        private HttpClient _client;

        public DocsApiClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            _client = new HttpClient(handler);
        }

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

        public async Task<JObject> GetScope(string scopeName)
        {
            var response = await CallService(HttpMethod.Get, $"api/Scope/{Uri.EscapeDataString(scopeName)}", null, false);
            if (String.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            return JObject.Parse(response);

        }

        public async Task<JObject> AddScope(string scopeName, string parentScopeId)
        {
            var payload = "{ \"Name\": \"" + scopeName + "\", ";
            if (parentScopeId != null)
            {
                payload +=  "\"ParentScopeId\": \"" + parentScopeId +
                    "\", \"IsRoot\": false }";
            }
            else
            {
                payload += "\"IsRoot\": true }";
            }

            var respsonse = await CallService(HttpMethod.Post, "api/scope", payload);
            return JObject.Parse(respsonse);
        }

        public async Task AddConfigProperties(string scopeName, JObject properties)
        {
            var response = await CallService(HttpMethod.Post, $"api/Scope/{scopeName}/ConfigValues", properties.ToString());
        }

        public async Task<JObject> GetCustomer(string code)
        {
            var response = await CallService(HttpMethod.Get, $"api/customer/{Uri.EscapeDataString(code)}", null, false);
            if (String.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            return JObject.Parse(response);
        }

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
