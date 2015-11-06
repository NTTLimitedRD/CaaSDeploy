using System;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which executes a post deployment script.
    /// </summary>
    public sealed class ExecuteScriptTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteScriptTask"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="scriptPath">The path to the scripts folder.</param>
        public ExecuteScriptTask(Resource resource, string scriptPath)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            Resource = resource;
            ScriptPath = scriptPath;
        }

        /// <summary>
        /// Gets the resource to execute the script for.
        /// </summary>
        public Resource Resource { get; private set; }

        /// <summary>
        /// Gets the path to the scripts folder.
        /// </summary>
        public string ScriptPath { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            throw new NotSupportedException("Post-Deploy script not supported.");
        }
    }
}
