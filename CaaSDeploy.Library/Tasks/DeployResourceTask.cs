using System;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which deploys a resource.
    /// </summary>
    public sealed class DeployResourceTask : ITask
    {
        /// <summary>
        /// The resource
        /// </summary>
        private readonly Resource _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployResourceTask"/> class.
        /// </summary>
        /// <param name="resource">The resource to deploy.</param>
        public DeployResourceTask(Resource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _resource = resource;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            await TokenHelper.SubstituteTokensInJObject(runtimeContext, taskContext, _resource.ResourceDefinition);
            var deployer = new ResourceDeployer(runtimeContext, _resource.ResourceId, _resource.ResourceType);
            var resourceLog = await deployer.DeployAndWait(_resource.ResourceDefinition);

            taskContext.Log.Resources.Add(resourceLog);
            taskContext.ResourcesProperties.Add(resourceLog.ResourceId, resourceLog.Details);

            if (resourceLog.DeploymentStatus == ResourceLogStatus.Failed)
            {
                taskContext.Log.Status = DeploymentLogStatus.Failed;
            }
        }
    }
}
