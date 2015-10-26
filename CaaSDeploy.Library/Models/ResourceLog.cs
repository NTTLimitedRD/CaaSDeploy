using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents the log entry for a single resource.
    /// </summary>
    public class ResourceLog
    {
        /// <summary>
        /// Gets or sets the deployment status.
        /// </summary>
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceLogStatus deploymentStatus { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceType resourceType { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        public string resourceId { get; set; }

        /// <summary>
        /// Gets or sets the CaaS identifier.
        /// </summary>
        public string caasId { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        [JsonIgnore]
        public JObject details { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public JObject error { get; set; }
    }
}
