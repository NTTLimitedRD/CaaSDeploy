using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Provides instance of <see cref="IHttpClient"/>.
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        /// Gets or sets the fake client. (We should really use IOC...)
        /// </summary>
        public static IHttpClient FakeClient { get; set; }

        /// <summary>
        /// Gets a new client.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <returns>The client.</returns>
        public static IHttpClient GetClient(CaasAccountDetails accountDetails)
        {
            if (FakeClient != null)
            {
                return FakeClient;
            }

            return GetProductionClient(accountDetails);
        }

        /// <summary>
        /// Gets the production client.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <returns>The client.</returns>
        private static IHttpClient GetProductionClient(CaasAccountDetails accountDetails)
        {
            var credentials = new NetworkCredential(accountDetails.UserName, accountDetails.Password);
            var handler = new HttpClientHandler { Credentials = credentials };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return new HttpClientAdapter(client);
        }
    }
}
