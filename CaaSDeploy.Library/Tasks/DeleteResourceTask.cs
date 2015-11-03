using System;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which deletes a resource.
    /// </summary>
    public sealed class DeleteResourceTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResourceTask"/> class.
        /// </summary>
        /// <param name="resourceLog">The resource log to delete.</param>
        public DeleteResourceTask(ResourceLog resourceLog)
        {
            if (resourceLog == null)
            {
                throw new ArgumentNullException(nameof(resourceLog));
            }

            ResourceLog = resourceLog;
        }

        /// <summary>
        /// Gets the log of the deployed resource to delete.
        /// </summary>
        public ResourceLog ResourceLog { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            if (ResourceLog.CaasId != null)
            {
                var deployer = new ResourceDeployer(runtimeContext, ResourceLog.ResourceId, ResourceLog.ResourceType);
                var resourceLog = await deployer.DeleteAndWait(ResourceLog.CaasId);

                taskContext.Log.Resources.Add(resourceLog);

                if (resourceLog.DeploymentStatus == ResourceLogStatus.Failed)
                {
                    taskContext.Log.Status = DeploymentLogStatus.Failed;
                }
            }
        }
    }
}
