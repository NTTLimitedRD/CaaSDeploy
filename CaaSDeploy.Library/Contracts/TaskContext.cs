using System.Collections.Generic;

using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// The task execution context provides template and runtime information across tasks.
    /// The object can be stored in a database between the execution of tasks.
    /// </summary>
    public sealed class TaskContext
    {
        /// <summary>
        /// Gets or sets the input parameters.
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the output parameters.
        /// </summary>
        public IDictionary<string, string> OutputParameters { get; set; }

        /// <summary>
        /// Gets or sets the resources properties.
        /// </summary>
        public IDictionary<string, JObject> ResourcesProperties { get; set; }

        /// <summary>
        /// Gets or sets the deployment log.
        /// </summary>
        public DeploymentLog Log { get; set; }
    }
}
