using System;
using System.Threading.Tasks;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using CaasDeploy.Library.Utilities;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which deploys a resource.
    /// </summary>
    internal sealed class DeployResourceTask : ITask
    {
        private readonly CaasAccountDetails _accountDetails;
        private readonly ILogProvider _logProvider;
        private readonly Resource _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployResourceTask"/> class.
        /// </summary>
        /// <param name="accountDetails">The CaaS account details.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="resource">The resource to deploy.</param>
        public DeployResourceTask(CaasAccountDetails accountDetails, ILogProvider logProvider, Resource resource)
        {
            if (accountDetails == null)
            {
                throw new ArgumentNullException(nameof(accountDetails));
            }

            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _accountDetails = accountDetails;
            _logProvider = logProvider;
            _resource = resource;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(TaskContext context)
        {
            TokenHelper.SubstituteTokensInJObject(_resource.resourceDefinition, context.Parameters, context.ResourcesProperties);
            var deployer = new ResourceDeployer(_resource.resourceId, _resource.resourceType, _accountDetails, _logProvider);
            var resourceLog = await deployer.DeployAndWait(_resource.resourceDefinition.ToString());

            context.Log.resources.Add(resourceLog);
            context.ResourcesProperties.Add(resourceLog.resourceId, resourceLog.details);

            if (resourceLog.deploymentStatus == DeploymentStatus.Failed)
            {
                context.Log.status = DeploymentLogStatus.Failed;
            }
        }
    }
}
