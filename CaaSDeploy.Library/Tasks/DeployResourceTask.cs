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
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The resource
        /// </summary>
        private readonly Resource _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployResourceTask"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="resource">The resource to deploy.</param>
        public DeployResourceTask(ILogProvider logProvider, Resource resource)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _logProvider = logProvider;
            _resource = resource;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="accountDetails">The account details.</param>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(CaasAccountDetails accountDetails, TaskContext context)
        {
            TokenHelper.SubstituteTokensInJObject(_resource.ResourceDefinition, context.Parameters, context.ResourcesProperties);
            var deployer = new ResourceDeployer(_logProvider, accountDetails, _resource.ResourceId, _resource.ResourceType);
            var resourceLog = await deployer.DeployAndWait(_resource.ResourceDefinition.ToString());

            context.Log.Resources.Add(resourceLog);
            context.ResourcesProperties.Add(resourceLog.ResourceId, resourceLog.Details);

            if (resourceLog.DeploymentStatus == ResourceLogStatus.Failed)
            {
                context.Log.Status = DeploymentLogStatus.Failed;
            }
        }
    }
}
