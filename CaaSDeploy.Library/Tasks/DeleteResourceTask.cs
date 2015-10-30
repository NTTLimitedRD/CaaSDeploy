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
        /// The resource
        /// </summary>
        private readonly ResourceLog _resourceLog;

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

            _resourceLog = resourceLog;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            if (_resourceLog.CaasId != null)
            {
                var deployer = new ResourceDeployer(runtimeContext, _resourceLog.ResourceId, _resourceLog.ResourceType);
                var resourceLog = await deployer.DeleteAndWait(_resourceLog.CaasId);

                taskContext.Log.Resources.Add(resourceLog);

                if (resourceLog.DeploymentStatus == ResourceLogStatus.Failed)
                {
                    taskContext.Log.Status = DeploymentLogStatus.Failed;
                }
            }
        }
    }
}
