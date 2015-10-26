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
    internal sealed class DeleteResourceTask : ITask
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The resource
        /// </summary>
        private readonly ResourceLog _resourceLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResourceTask"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="resourceLog">The resource log to delete.</param>
        public DeleteResourceTask(ILogProvider logProvider, ResourceLog resourceLog)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (resourceLog == null)
            {
                throw new ArgumentNullException(nameof(resourceLog));
            }

            _logProvider = logProvider;
            _resourceLog = resourceLog;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(TaskContext context)
        {
            if (_resourceLog.CaasId != null)
            {
                var deployer = new ResourceDeployer(_logProvider, context.AccountDetails, _resourceLog.ResourceId, _resourceLog.ResourceType);
                await deployer.DeleteAndWait(_resourceLog.CaasId);

                context.Log.Resources.Add(_resourceLog);
            }
        }
    }
}
