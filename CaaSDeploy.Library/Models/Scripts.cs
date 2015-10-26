using Newtonsoft.Json;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents a resource scripts block.
    /// </summary>
    public class Scripts
    {
        /// <summary>
        /// Gets or sets the path to the bundle file.
        /// </summary>
        [JsonProperty("bundleFile")]
        public string BundleFile { get; set; }

        /// <summary>
        /// Gets or sets the on deploy.
        /// </summary>
        [JsonProperty("onDeploy")]
        public string OnDeploy { get; set; }
    }
}
