using CaasDeploy.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CaasDeploy.Library
{
    public static class CaasAuthentication
    {
        private const string authUrl = "/oec/0.9/myaccount";

        public static async Task<CaasAccountDetails> Authenticate(string userName, string password, string region)
        {
            var credentials = new NetworkCredential(userName, password);
            var handler = new HttpClientHandler { Credentials = credentials };

            using (var client = new HttpClient(handler))
            {
                string url = Configuration.ApiBaseUrls[region] + authUrl;
                var responseSteam = await client.GetStreamAsync(url);
                var xdoc = XDocument.Load(responseSteam);
                XNamespace ns5 = "http://oec.api.opsource.net/schemas/directory";
                var orgId = xdoc.Root.Element(ns5 + "orgId").Value;
                return new CaasAccountDetails
                {
                    UserName = userName,
                    Password = password,
                    OrgId = orgId,
                    Region = region,
                };
            }
        }
    }
}
