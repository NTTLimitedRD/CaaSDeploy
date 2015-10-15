using System.Collections.Generic;

using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// The task execution context provides template and runtime information accross tasks.
    /// </summary>
    public sealed class TaskContext
    {
        /// <summary>
        /// Gets or sets the script path.
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        public IEnumerable<Resource> Resources { get; set; }

        /// <summary>
        /// Gets or sets the resources properties.
        /// </summary>
        public Dictionary<string, JObject> ResourcesProperties { get; set; }

        /// <summary>
        /// Gets or sets the deployment log.
        /// </summary>
        public DeploymentLog Log { get; set; }
    }
}
