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
            { "NetworkDomain", new CaasApiUrls { DeployUrl = "/network/deployNetworkDomain", GetUrl = "/network/networkDomain/{0}", DeleteUrl = "/network/deleteNetworkDomain" } },
            { "VLAN", new CaasApiUrls { DeployUrl = "/network/deployVlan", GetUrl = "/network/vlan/{0}", DeleteUrl = "/network/deleteVlan" } },
            { "Server", new CaasApiUrls { DeployUrl = "/server/deployServer", GetUrl = "/server/server/{0}", DeleteUrl = "/server/deleteServer" } },
        };

        // Move this to config...
        private Dictionary<string, string> _apiBaseUrls = new Dictionary<string, string>()
        {
            { "NA", "https://api-na.dimensiondata.com" },
            { "EU", "https://api-eu.dimensiondata.com" },
            { "AU", "https://api-au.dimensiondata.com" },
            { "AF", "https://api-mea.dimensiondata.com" },
            { "AP", "https://api-ap.dimensiondata.com" },
            { "SA", "https://api-latam.dimensiondata.com" },
            { "CA", "https://api-canada.dimensiondata.com" },
            { "CANBERRA", "https://api-canberra.dimensiondata.com" },
        };

        private const string _mcp2UrlStem = "/caas/2.0";
        private string _resourceType;
        private CaasApiUrls _resourceApi;
        private CaasAccountDetails _accountDetails;
        private string _apiBaseUrl;
        private const int _pollingDelaySeconds = 30;
        private const int _pollingTimeOutMinutes = 20;

        public ResourceDeployer(string resourceType, string region, CaasAccountDetails accountDetails)
        {
            _resourceType = resourceType;
            _resourceApi = _resourceApis[resourceType];
            _apiBaseUrl = _apiBaseUrls[region];
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
            using (var client = GetHttpClient())
            {
                var url = GetApiUrl(_resourceApi.DeployUrl);
                var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
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
                response.EnsureSuccessStatusCode();
                var jsonResponse = JObject.Parse(responseBody);
                var properties = new Dictionary<string, string>();
                foreach (var jprop in jsonResponse.Properties())
                {
                    properties.Add(jprop.Name, jprop.Value.ToString());
                }
                return properties;
                
            }
        }

        private async Task Delete(string id)
        {
            using (var client = GetHttpClient())
            {
                var url = GetApiUrl(_resourceApi.DeleteUrl);
                string jsonPayload = String.Format("{{ \"id\": \"{0}\" }}", id);
                var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
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
                    return props;
                }
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
                catch (HttpRequestException)
                {
                    // Check detail
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(_pollingDelaySeconds));
            }

        }
    }
}
