using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents an existing CaaS resource a new resource depends on.
    /// </summary>
    public class ExistingResource
    {
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
    }
}
