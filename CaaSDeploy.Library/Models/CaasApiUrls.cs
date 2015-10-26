namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Contains the action URLs for a particular type of CaaS resource.
    /// </summary>
    public class CaasApiUrls
    {
        /// <summary>
        /// Gets or sets the deploy URL.
        /// </summary>
        public string DeployUrl { get; set; }

        /// <summary>
        /// Gets or sets the get URL.
        /// </summary>
        public string GetUrl { get; set; }

        /// <summary>
        /// Gets or sets the delete URL.
        /// </summary>
        public string DeleteUrl { get; set; }

        /// <summary>
        /// Gets or sets the list URL.
        /// </summary>
        public string ListUrl { get; set; }

        /// <summary>
        /// Gets or sets the edit URL.
        /// </summary>
        public string EditUrl { get; set; }
    }
}
