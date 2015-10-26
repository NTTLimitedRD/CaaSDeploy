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
        [JsonProperty("deploymentStatus")]
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceLogStatus DeploymentStatus { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        [JsonProperty("resourceType")]
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the CaaS identifier.
        /// </summary>
        [JsonProperty("caasId")]
        public string CaasId { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        [JsonProperty("error")]
        public JObject Error { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        [JsonIgnore]
        public JObject Details { get; set; }
    }
}
