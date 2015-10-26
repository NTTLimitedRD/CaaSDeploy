using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents an existing CaaS resource a new resource depends on.
    /// </summary>
    public class ExistingResource
    {
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
    }
}
