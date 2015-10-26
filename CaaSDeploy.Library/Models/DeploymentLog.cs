using System;
using System.Collections.Generic;

using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents the entire log of a deployment operation.
    /// </summary>
    public class DeploymentLog
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(JsonEnumConverter))]
        public DeploymentLogStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the deployment time.
        /// </summary>
        [JsonProperty("deploymentTime")]
        public DateTime DeploymentTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        [JsonProperty("resources")]
        public List<ResourceLog> Resources { get; set; }
    }
}
