using System.Collections.Generic;

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
        public DeploymentTemplateMetadata metadata { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public JObject parameters { get; set; }

        /// <summary>
        /// Gets or sets the existing resources the template depends on.
        /// </summary>
        public List<ExistingResource> existingResources { get; set; }

        /// <summary>
        /// Gets or sets the resources to deploy.
        /// </summary>
        public List<Resource> resources { get; set; }

        /// <summary>
        /// Gets or sets the orchestration.
        /// </summary>
        public JObject orchestration { get; set; }
    }
}