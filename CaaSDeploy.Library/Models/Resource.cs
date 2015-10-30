using System.Collections.Generic;

using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents a deployment resource.
    /// </summary>
    public class Resource
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
        /// Gets or sets the CaaS identifier if the resource already exists.
        /// </summary>
        [JsonProperty("existingCaasId")]
        public string ExistingCaasId { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the existing or new resources this resource depends on.
        /// </summary>
        [JsonProperty("dependsOn")]
        public List<string> DependsOn { get; set; }

        /// <summary>
        /// Gets or sets the resource definition.
        /// </summary>
        [JsonProperty("resourceDefinition")]
        public JObject ResourceDefinition { get; set; }

        /// <summary>
        /// Gets or sets the scripts to run.
        /// </summary>
        [JsonProperty("scripts")]
        public Scripts Scripts { get; set; }
    }
}