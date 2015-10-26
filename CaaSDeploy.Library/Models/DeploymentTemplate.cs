using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents an entire deployment template.
    /// </summary>
    public class DeploymentTemplate
    {
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        [JsonProperty("metadata")]
        public DeploymentTemplateMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        [JsonProperty("parameters")]
        public JObject Parameters { get; set; }

        /// <summary>
        /// Gets or sets the existing resources the template depends on.
        /// </summary>
        [JsonProperty("existingResources")]
        public List<ExistingResource> ExistingResources { get; set; }

        /// <summary>
        /// Gets or sets the resources to deploy.
        /// </summary>
        [JsonProperty("resources")]
        public List<Resource> Resources { get; set; }

        /// <summary>
        /// Gets or sets the orchestration.
        /// </summary>
        [JsonProperty("orchestration")]
        public JObject Orchestration { get; set; }
    }
}