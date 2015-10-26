namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Contains CaaS account details after successful authentication.
    /// </summary>
    public class CaasAccountDetails
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the API base URL for the region.
        /// </summary>
        public string BaseUrl { get; set; }
    }
}
