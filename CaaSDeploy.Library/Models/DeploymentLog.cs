using System;
using System.Collections.Generic;

using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace CaasDeploy.Library.Models
{
    /// <summary>
    /// Represents the entire log of a deployment operation.
    /// </summary>
    public class DeploymentLog
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        [JsonConverter(typeof(JsonEnumConverter))]
        public DeploymentLogStatus status { get; set; }

        /// <summary>
        /// Gets or sets the deployment time.
        /// </summary>
        public DateTime deploymentTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        public string templateName { get; set; }

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        public List<ResourceLog> resources { get; set; }
    }
}
