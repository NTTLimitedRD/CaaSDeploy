﻿using System;
using System.Threading.Tasks;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which deletes a resource.
    /// </summary>
    internal sealed class DeleteResourceTask : ITask
    {
        private readonly CaasAccountDetails _accountDetails;
        private readonly ILogProvider _logProvider;
        private readonly ResourceLog _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResourceTask"/> class.
        /// </summary>
        /// <param name="accountDetails">The CaaS account details.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="resource">The resource to deploy.</param>
        public DeleteResourceTask(CaasAccountDetails accountDetails, ILogProvider logProvider, ResourceLog resource)
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
            if (_resource.details != null)
            {
                var deployer = new ResourceDeployer(_logProvider, _accountDetails, _resource.resourceId, _resource.resourceType);
                var caasId = _resource.details["id"].Value<string>();
                await deployer.DeleteAndWait(caasId);

                context.Log.resources.Add(_resource);
            }
        }
    }
}