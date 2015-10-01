using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Docs
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

    }
}
