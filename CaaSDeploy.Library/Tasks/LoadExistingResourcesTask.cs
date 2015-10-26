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
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The existing resources
        /// </summary>
        private readonly IList<ExistingResource> _existingResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadExistingResourcesTask"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="existingResources">The existing resources to load.</param>
        public LoadExistingResourcesTask(ILogProvider logProvider, IList<ExistingResource> existingResources)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (existingResources == null)
            {
                throw new ArgumentNullException(nameof(existingResources));
            }

            _logProvider = logProvider;
            _existingResources = existingResources;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(CaasAccountDetails accountDetails, TaskContext context)
        {
            if (_existingResources == null)
            {
                return;
            }

            foreach (var existingResource in _existingResources)
            {
                existingResource.CaasId = TokenHelper.SubstitutePropertyTokensInString(existingResource.CaasId, context.Parameters);
                var deployer = new ResourceDeployer(_logProvider, accountDetails, existingResource.ResourceId, existingResource.ResourceType);
                var resource = await deployer.Get(existingResource.CaasId);
                context.ResourcesProperties.Add(existingResource.ResourceId, resource);
            }
        }
    }
}
