using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;

namespace CaasDeploy.Library
{
    /// <summary>
    /// Performs authentication with CaaS.
    /// </summary>
    public static class CaasAuthentication
    {
        /// <summary>
        /// The authentication base URL.
        /// </summary>
        private const string authUrl = "/oec/0.9/myaccount";

        /// <summary>
        /// Authenticates a user.
        /// </summary>
        /// <param name="config">The compute configuration.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="regionKey">The region key.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public static async Task<CaasAccountDetails> Authenticate(IComputeConfiguration config, string userName, string password, string regionKey)
        {
            var credentials = new NetworkCredential(userName, password);
            var handler = new HttpClientHandler { Credentials = credentials };

            using (var client = new HttpClient(handler))
            {
                var region = config.GetRegions().FirstOrDefault(r => r.Key == regionKey);
                if (region == null)
                {
                    throw new ArgumentException($"The region with key '{regionKey}' does not exist in the app.config file.");
                }

                var responseSteam = await client.GetStreamAsync(region.BaseUrl);
                var xdoc = XDocument.Load(responseSteam);
                XNamespace ns5 = "http://oec.api.opsource.net/schemas/directory";
                var orgId = xdoc.Root.Element(ns5 + "orgId").Value;
                return new CaasAccountDetails
                {
                    UserName = userName,
                    Password = password,
                    OrgId = orgId,
                    Region = regionKey,
                    BaseUrl = region.BaseUrl
                };
            }
        }
    }
}
