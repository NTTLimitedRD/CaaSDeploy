using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class ResourceDeployer
    {
        private Dictionary<string, CaasApiUrls> _resourceApis = new Dictionary<string, CaasApiUrls>
        {
            { "NetworkDomain", new CaasApiUrls { DeployUrl = "/network/deployNetworkDomain", GetUrl = "/network/networkDomain/{0}", ListUrl="/network/networkDomain?name={0}", DeleteUrl = "/network/deleteNetworkDomain" } },
            { "VLAN", new CaasApiUrls { DeployUrl = "/network/deployVlan", GetUrl = "/network/vlan/{0}", ListUrl="/network/vlan?name={0}", DeleteUrl = "/network/deleteVlan" } },
            { "Server", new CaasApiUrls { DeployUrl = "/server/deployServer", GetUrl = "/server/server/{0}", ListUrl = "/server/server?name={0}", DeleteUrl = "/server/deleteServer" } },
        };


        private const string _mcp2UrlStem = "/caas/2.0";
        private string _resourceId;
        private string _resourceType;
        private CaasApiUrls _resourceApi;
        private CaasAccountDetails _accountDetails;
        private string _apiBaseUrl;
        private const int _pollingDelaySeconds = 30;
        private const int _pollingTimeOutMinutes = 20;

        public ResourceDeployer(string resourceId, string resourceType, string region, CaasAccountDetails accountDetails)
        {
            _resourceId = resourceId;
            _resourceType = resourceType;
            _resourceApi = _resourceApis[resourceType];
            _apiBaseUrl = Configuration.ApiBaseUrls[region];
            _accountDetails = accountDetails;
        }

        private HttpClient GetHttpClient()
        {
            var credentials = new NetworkCredential(_accountDetails.UserName, _accountDetails.Password);
            var handler = new HttpClientHandler { Credentials = credentials };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client; 
        }

        private async Task<string> Deploy(string jsonPayload)
        {
            Console.Write("Deploying {0}: '{1}'", _resourceType, _resourceId);
            using (var client = GetHttpClient())
            {
                var url = GetApiUrl(_resourceApi.DeployUrl);
                var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);
                var jsonResponse = JObject.Parse(responseBody);
                var info = (JArray)jsonResponse["info"];
                // TODO: Check if we ever get more than 1 
                return info[0]["value"].Value<string>();
            }
        }

        public async Task<Dictionary<string, string>> DeployAndWait(string jsonPayload)
        {
            var id = await Deploy(jsonPayload);
            return await WaitForDeploy(id);
        }

        public async Task<Dictionary<string, string>> Get(string id)
        {
            using (var client = GetHttpClient())
            {
                var url = String.Format(GetApiUrl(_resourceApi.GetUrl), id);
                var response = await client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);
                var jsonResponse = JObject.Parse(responseBody);
                var properties = new Dictionary<string, string>();
                foreach (var jprop in jsonResponse.Properties())
                {
                    properties.Add(jprop.Name, jprop.Value.ToString());
                }
                return properties;
                
            }
        }

        public async Task<string> GetResourceIdByName(string name)
        {
            using (var client = GetHttpClient())
            {
                var url = String.Format(GetApiUrl(_resourceApi.ListUrl), name);
                var response = await client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);
                var jsonResponse = JObject.Parse(responseBody);
                var results = (JArray) jsonResponse.First.Children().First();
                if (results.Count == 0)
                {
                    return null;
                }
                else if (results.Count > 1)
                {
                    throw new InvalidOperationException("More than 1 matching item found.");
                }

                return results[0]["id"].Value<string>();

            }
        }

        private async Task Delete(string id)
        {
            Console.Write("Deleting {0}: '{1}'", _resourceType, _resourceId);
            using (var client = GetHttpClient())
            {
                var url = GetApiUrl(_resourceApi.DeleteUrl);
                string jsonPayload = String.Format("{{ \"id\": \"{0}\" }}", id);
                var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                await ThrowForHttpFailure(response);
            }
        }

        private async Task ThrowForHttpFailure(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new WebException(responseBody);
            }
        }

        public async Task DeleteAndWait(string id)
        {
            await Delete(id);
            await WaitForDelete(id);
        }

        private string GetApiUrl(string url)
        {
            return _apiBaseUrl + _mcp2UrlStem + "/" + _accountDetails.OrgId + url;
        }

        public async Task<Dictionary<string, string>> WaitForDeploy(string id)
        {
            DateTime startTime = DateTime.Now;
            while(true)
            {
                if ((DateTime.Now - startTime).TotalMinutes >= _pollingTimeOutMinutes)
                {
                    throw new TimeoutException(String.Format(
                        "Timed out waiting to create {0} with id {1}", _resourceType, id));
                }

                var props = await Get(id);
                if (props["state"] == "NORMAL")
                {
                    Console.WriteLine("Done!");
                    return props;
                }
                Console.Write(".");
                await Task.Delay(TimeSpan.FromSeconds(_pollingDelaySeconds));
            }

        }

        public async Task WaitForDelete(string id)
        {
            DateTime startTime = DateTime.Now;
            while (true)
            {
                if ((DateTime.Now - startTime).TotalMinutes >= _pollingTimeOutMinutes)
                {
                    throw new TimeoutException(String.Format(
                        "Timed out waiting to delete {0} with id {1}", _resourceType, id));
                }

                try
                {
                    var props = await Get(id);
                }
                catch (WebException)
                {
                    // Check detail
                    Console.WriteLine("Done!");
                    return;
                }
                Console.Write(".");
                await Task.Delay(TimeSpan.FromSeconds(_pollingDelaySeconds));
            }

        }
    }
}
