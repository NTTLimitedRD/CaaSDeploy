using System.Collections.Generic;

using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.Library.Models
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
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public IList<string> BundleFile { get; set; }

        /// <summary>
        /// Gets or sets the on deploy.
        /// </summary>
        [JsonProperty("onDeploy")]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public IList<string> OnDeploy { get; set; }
    }
}
