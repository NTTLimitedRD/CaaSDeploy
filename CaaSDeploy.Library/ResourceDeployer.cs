using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            { "NetworkDomain", new CaasApiUrls { DeployUrl = "/network/deployNetworkDomain", GetUrl = "/network/networkDomain/{0}", ListUrl="/network/networkDomain?name={0}", DeleteUrl = "/network/deleteNetworkDomain", EditUrl = "/network/editNetworkDomain" } },
            { "Vlan", new CaasApiUrls { DeployUrl = "/network/deployVlan", GetUrl = "/network/vlan/{0}", ListUrl="/network/vlan?name={0}", DeleteUrl = "/network/deleteVlan", EditUrl = "/network/editVlan" } },
            { "Server", new CaasApiUrls { DeployUrl = "/server/deployServer", GetUrl = "/server/server/{0}", ListUrl = "/server/server?name={0}", DeleteUrl = "/server/deleteServer", EditUrl = null } },
            { "FirewallRule", new CaasApiUrls { DeployUrl = "/network/createFirewallRule", GetUrl = "/network/firewallRule/{0}", ListUrl = "/network/firewallRule?name={0}&networkDomainId={1}", DeleteUrl = "/network/deleteFirewallRule", EditUrl = "/network/editFirewallRule" } },
            { "PublicIpBlock", new CaasApiUrls { DeployUrl = "/network/addPublicIpBlock", GetUrl = "/network/publicIpBlock/{0}", ListUrl = null, DeleteUrl = "/network/removePublicIpBlock", EditUrl= null } },
            { "NatRule", new CaasApiUrls { DeployUrl = "/network/createNatRule", GetUrl = "/network/natRule/{0}", ListUrl = "/network/natRule?networkDomainId={0}&internalIp={1}", DeleteUrl = "/network/deleteNatRule", EditUrl = null } },
            { "VirtualListener", new CaasApiUrls { DeployUrl = "/networkDomainVip/createVirtualListener", GetUrl = "/networkDomainVip/virtualListener/{0}", ListUrl = "/networkDomainVip/virtualListener?name={0}", DeleteUrl = "/networkDomainVip/deleteVirtualListener", EditUrl = "/networkDomainVip/editVirtualListener" } },
            { "Pool", new CaasApiUrls { DeployUrl = "/networkDomainVip/createPool", GetUrl = "/networkDomainVip/pool/{0}", ListUrl = "/networkDomainVip/pool?name={0}", DeleteUrl = "/networkDomainVip/deletePool", EditUrl = "/networkDomainVip/editPool" } },
            { "Node", new CaasApiUrls { DeployUrl = "/networkDomainVip/createNode", GetUrl = "/networkDomainVip/node/{0}", ListUrl = "/networkDomainVip/node?name={0}", DeleteUrl = "/networkDomainVip/deleteNode",  EditUrl = "/networkDomainVip/editNode" } },
            { "PoolMember", new CaasApiUrls { DeployUrl = "/networkDomainVip/addPoolMember", GetUrl = "/networkDomainVip/poolMember/{0}", ListUrl = "/networkDomainVip/poolMember?poolId={0}&nodeId={1}", DeleteUrl = "/networkDomainVip/removePoolMember", EditUrl= "/networkDomainVip/editPoolMember"} },
        };

        private Dictionary<string, string[]> _propertiesNotSupportedForEdit = new Dictionary<string, string[]>
        {
            { "NetworkDomain", new[] { "datacenterId" } },
            { "Vlan", new[] { "networkDomainId", "privateIpv4BaseAddress" } },
            { "Server", new[] { "networkDomainId" } },
            { "FirewallRule", new[] { "networkDomainId", "name" } },
            { "PublicIpBlock", new[] { "networkDomainId" } },
            { "NatRule", new[] { "networkDomainId" } },
            { "VirtualListener", new[] { "networkDomainId" } },
            { "Pool", new[] { "networkDomainId", "name" } },
            { "Node", new[] { "networkDomainId" } },
            { "PoolMember", new[] { "networkDomainId" } },
        };


        private const string _mcp2UrlStem = "/caas/2.0";
        private string _resourceId;
        private string _resourceType;
        private CaasApiUrls _resourceApi;
        private CaasAccountDetails _accountDetails;
        private string _apiBaseUrl;
        private const int _pollingDelaySeconds = 30;
        private const int _pollingTimeOutMinutes = 20;
        private ILogProvider _logWriter;

        public ResourceDeployer(string resourceId, string resourceType, CaasAccountDetails accountDetails, ILogProvider logWriter)
        {
            _resourceId = resourceId;
            _resourceType = resourceType;
            _resourceApi = _resourceApis[resourceType];
            _apiBaseUrl = Configuration.ApiBaseUrls[accountDetails.Region];
            _accountDetails = accountDetails;
            _logWriter = logWriter;
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
            _logWriter.LogMessage($"Deploying {_resourceType}: '{_resourceId}' ");

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

        public async Task<ResourceLog> DeployAndWait(string jsonPayload)
        {
            var response = new ResourceLog() { resourceId = _resourceId, resourceType = _resourceType };

            try
            {
                if (_resourceApi.ListUrl != null)
                {
                    var resourceDefinition = JObject.Parse(jsonPayload);
                    var ids = GetResourceIdentifiers(resourceDefinition);
                    var existingResourceDetails = (await GetResourceByIdentifiers(ids)).SingleOrDefault();
                    if (existingResourceDetails != null)
                    {
                        if (_resourceApi.EditUrl == null)
                        {
                            _logWriter.LogMessage($"Resource '{_resourceId}' already exists and cannot be updated. Using existing resource even if its definition doesn't match the template.");
                            response.details = existingResourceDetails;
                            response.deploymentStatus = ResourceLog.DeploymentStatusUsedExisting;
                            return response;
                        }
                        else
                        {
                            var existingId = existingResourceDetails["id"].Value<string>();
                            await UpdateExistingResource(existingId, resourceDefinition);
                            response.details = await Get(existingId);
                            response.deploymentStatus = ResourceLog.DeploymentStatusUpdated;
                            return response;
                        }
                    }
                }

                var id = await Deploy(jsonPayload);
                response.details = await WaitForDeploy(id);
                response.deploymentStatus = ResourceLog.DeploymentStatusDeployed;
                return response;
            }
            catch (CaasException ex)
            {
                response.deploymentStatus = ResourceLog.DeploymentStatusFailed;
                response.error = ex.FullResponse;
                return response;
            }

        }

        private async Task UpdateExistingResource(string existingId, JObject resourceDefinition)
        {
            _logWriter.LogMessage($"Updating existing {_resourceType}: '{_resourceId}' ");

            using (var client = GetHttpClient())
            {
                resourceDefinition.AddFirst(new JProperty("id", existingId));
                RemovePropertiesUnsupportedForEdit(resourceDefinition);
                var url = GetApiUrl(_resourceApi.EditUrl);
                var content = new StringContent(resourceDefinition.ToString(), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // CaaS bug is causing errors if charset is preset in the content-type header
                var response = await client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);

            }
        }

        private void RemovePropertiesUnsupportedForEdit(JObject resourceDefinition)
        {
            foreach (var prop in _propertiesNotSupportedForEdit[_resourceType])
            {
                resourceDefinition.Remove(prop);
            }
        }

        public async Task<JObject> Get(string id)
        {
            using (var client = GetHttpClient())
            {
                var url = String.Format(GetApiUrl(_resourceApi.GetUrl), id);
                var response = await client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);
                var jsonResponse = JObject.Parse(responseBody);
                return jsonResponse;
                
            }
        }

        private async Task<bool> Delete(string id) // Returns true if waiting is required
        {
            _logWriter.LogMessage($"Deleting {_resourceType}: '{_resourceId}' (ID: {id}) ");
            using (var client = GetHttpClient())
            {
                try
                {
                    var url = GetApiUrl(_resourceApi.DeleteUrl);
                    string jsonPayload = String.Format("{{ \"id\": \"{0}\" }}", id);
                    var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                    await ThrowForHttpFailure(response);
                    return true;
                }
                catch (CaasException ex)
                {
                    // Check detail
                    if (ex.ResponseCode == "RESOURCE_NOT_FOUND")
                    {
                        _logWriter.LogMessage("Not found.");
                        return false;
                    }
                    throw;
                }
            }
        }

        private async Task ThrowForHttpFailure(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new CaasException(responseBody);
            }
        }

        public async Task DeleteAndWait(string id)
        {
            bool wait = await Delete(id);
            if (wait)
            {
                await WaitForDelete(id);
            }
        }

        private string GetApiUrl(string url)
        {
            return _apiBaseUrl + _mcp2UrlStem + "/" + _accountDetails.OrgId + url;
        }

        public async Task<JObject> WaitForDeploy(string id)
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
                if (props["state"].Value<string>() == "NORMAL")
                {
                    _logWriter.CompleteProgress();
                    return props;
                }
                _logWriter.IncrementProgress();
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
                catch (CaasException ex)
                {
                    // Check detail
                    if (ex.ResponseCode == "RESOURCE_NOT_FOUND")
                    {
                        _logWriter.CompleteProgress();
                        return;
                    }
                    throw;
                }
                _logWriter.IncrementProgress();
                await Task.Delay(TimeSpan.FromSeconds(_pollingDelaySeconds));
            }

        }

        /// <summary>
        /// Retrieves the values from the template resource definition that can be used to uniquely identify an
        /// alredy deployed resource, in the order specified in the ListUrl parameters. 
        /// </summary>
        /// <param name="resourceDefinition">The JSON resource definition from the template</param>
        /// <returns>The list of parameter values to be used for the List API call</returns>
        public string[] GetResourceIdentifiers(JObject resourceDefinition)
        {
            if (_resourceType == "PoolMember")
            {
                return new[] { resourceDefinition["poolId"].Value<string>(), resourceDefinition["nodeId"].Value<string>(), };
            }
            else if (_resourceType == "NatRule")
            {
                return new[] { resourceDefinition["networkDomainId"].Value<string>(), resourceDefinition["internalIp"].Value<string>()};
            }
            else if (_resourceType == "FirewallRule")
            {
                return new[] { resourceDefinition["name"].Value<string>(), resourceDefinition["networkDomainId"].Value<string>()};
            }
            return new[] { resourceDefinition["name"].Value<string>() };
        }

        public async Task<IEnumerable<JObject>> GetResourceByIdentifiers(string[] ids)
        {
            if (_resourceApi.ListUrl == null)
            {
                // Some resource types can't be retrieved just by name
                return null;
            }

            using (var client = GetHttpClient())
            {
                var url = String.Format(GetApiUrl(_resourceApi.ListUrl), ids);
                var response = await client.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                await ThrowForHttpFailure(response);
                var jsonResponse = JObject.Parse(responseBody);
                var results = (JArray)jsonResponse.First.Children().First();
                return results.Select(jv => (JObject)jv).ToArray();
            }
        }
    }
}
