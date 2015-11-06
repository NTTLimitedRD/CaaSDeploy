using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which loads existing resources.
    /// </summary>
    public sealed class LoadExistingResourcesTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadExistingResourcesTask"/> class.
        /// </summary>
        /// <param name="existingResources">The existing resources to load.</param>
        public LoadExistingResourcesTask(IList<Resource> existingResources)
        {
            if (existingResources == null)
            {
                throw new ArgumentNullException(nameof(existingResources));
            }

            ExistingResources = existingResources;
        }

        /// <summary>
        /// Gets the existing resources to load.
        /// </summary>
        public IList<Resource> ExistingResources { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            if (ExistingResources == null)
            {
                return;
            }

            foreach (var existingResource in ExistingResources)
            {
                existingResource.ExistingCaasId = await Macro.SubstituteTokensInString(runtimeContext, taskContext, existingResource.ExistingCaasId);
                var deployer = new ResourceDeployer(runtimeContext, existingResource.ResourceId, existingResource.ResourceType);
                var resource = await deployer.Get(existingResource.ExistingCaasId);
                taskContext.ResourcesProperties.Add(existingResource.ResourceId, resource);
            }
        }
    }
}
