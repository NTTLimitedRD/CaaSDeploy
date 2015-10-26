using System.Collections.Generic;

using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents a deployment resource.
    /// </summary>
    public class Resource
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
        /// Gets or sets the identifiers of the existing or new resources this resource depends on.
        /// </summary>
        public List<string> dependsOn { get; set; }

        /// <summary>
        /// Gets or sets the resource definition.
        /// </summary>
        public JObject resourceDefinition { get; set; }

        /// <summary>
        /// Gets or sets the scripts to run.
        /// </summary>
        public Scripts scripts { get; set; }
    }
}