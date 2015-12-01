using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Macros
{
    /// <summary>
    /// A macro to retrieve the next available public IP and create a new public IP block if necessary.
    /// </summary>
    public sealed class NextAvailablePublicIpMacro : IMacro
    {
        /// <summary>
        /// The macro regex.
        /// </summary>
        private static readonly Regex ImageRegex = new Regex(@"\$nextAvailablePublicIP\['([^']*)'\]");

        /// <summary>
        /// Substitutes the property tokens in the supplied string.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="input">The input string.</param>
        /// <returns>The substituted string</returns>
        public async Task<string> SubstituteTokensInString(RuntimeContext runtimeContext, TaskContext taskContext, string input)
        {
            string output = input;

            var matches = ImageRegex.Matches(input);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string networkDomainId = match.Groups[1].Value;

                    var publicIps = await ListAvailablePublicIps(runtimeContext.AccountDetails, networkDomainId);
                    if (publicIps.Count == 0)
                    {
                        await AddPublicIpBlock(runtimeContext.AccountDetails, networkDomainId);
                        publicIps = await ListAvailablePublicIps(runtimeContext.AccountDetails, networkDomainId);
                    }

                    if (publicIps.Count == 0)
                    {
                        throw new MacroException("No more public IPs available.");
                    }

                    output = publicIps[0];
                }
            }

            return output;
        }

        /// <summary>
        /// Lists the available public IP addresses.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <param name="networkDomainId">The network domain identifier.</param>
        /// <returns></returns>
        private async Task<List<string>> ListAvailablePublicIps(CaasAccountDetails accountDetails, string networkDomainId)
        {
            var result = new List<string>();

            using (var client = HttpClientFactory.GetClient(accountDetails, "application/json"))
            {
                // Get the reserved public IPs.
                var url = string.Format("{0}/caas/2.0/{1}/network/reservedPublicIpv4Address?networkDomainId={2}", accountDetails.BaseUrl, accountDetails.OrgId, networkDomainId);
                var response = await client.GetAsync(url);
                response.ThrowForHttpFailure();

                var responseBody = await response.Content.ReadAsStringAsync();
                var document = JObject.Parse(responseBody);
                var reservedPublicIps = (document.Property("ip").Value as JArray).Cast<JObject>()
                    .Select(e => e["value"].Value<string>())
                    .ToList();

                // Get the public IP blocks.
                url = string.Format("{0}/caas/2.0/{1}/network/publicIpBlock?networkDomainId={2}", accountDetails.BaseUrl, accountDetails.OrgId, networkDomainId);
                response = await client.GetAsync(url);
                response.ThrowForHttpFailure();

                responseBody = await response.Content.ReadAsStringAsync();
                document = JObject.Parse(responseBody);
                var ipBlocks = (document.Property("publicIpBlock").Value as JArray).Cast<JObject>()
                    .Select(e => new
                    {
                        BaseIp = e["baseIp"].Value<string>(),
                        Size = e["size"].Value<int>()
                    })
                    .ToList();

                // Get the available public IPs.
                foreach (var ipBlock in ipBlocks)
                {
                    for (int offset = 0; offset < ipBlock.Size; offset++)
                    {
                        var segments = ipBlock.BaseIp.Split('.');
                        var segment = int.Parse(segments.Last()) + offset;
                        if (segment > 255)
                        {
                            segment -= 256;
                        }

                        segments[3] = segment.ToString();
                        var ipAddress = string.Join(".", segments);

                        if (!reservedPublicIps.Contains(ipAddress))
                        {
                            result.Add(ipAddress);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a new public IP address block.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <param name="networkDomainId">The network domain identifier.</param>
        /// <returns></returns>
        private async Task AddPublicIpBlock(CaasAccountDetails accountDetails, string networkDomainId)
        {
            using (var client = HttpClientFactory.GetClient(accountDetails, "application/json"))
            {
                var url = string.Format("{0}/caas/2.0/{1}/network/addPublicIpBlock", accountDetails.BaseUrl, accountDetails.OrgId);
                var request = new JObject(new JProperty("networkDomainId", networkDomainId));
                var response = await client.PostAsync(url, new StringContent(request.ToString(), Encoding.UTF8, "application/json"));
                response.ThrowForHttpFailure();
            }
        }
    }
}
