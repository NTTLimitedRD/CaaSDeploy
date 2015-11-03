using System.Net;

namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Contains CaaS account details after successful authentication.
    /// </summary>
    public class CaasAccountDetails
    {
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// Gets or sets the API base URL for the region.
        /// </summary>
        public string BaseUrl { get; set; }
    }
}
