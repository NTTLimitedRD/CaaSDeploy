using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using CaasDeploy.Library.Tasks;
using CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library
{
    /// <summary>
    /// Builds task lists and contexts from deployment documents.
    /// </summary>
    public sealed class TaskBuilder
    {
        private readonly ILogProvider _logProvider;
        private readonly CaasAccountDetails _accountDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBuilder"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="accountDetails">The account details.</param>
        public TaskBuilder(ILogProvider logProvider, CaasAccountDetails accountDetails)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (accountDetails == null)
            {
                throw new ArgumentNullException(nameof(accountDetails));
            }

            _logProvider = logProvider;
            _accountDetails = accountDetails;
        }

        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="parametersFilePath">The parameters file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor GetDeploymentTasks(string templateFilePath, string parametersFilePath)
        {
            var template = TemplateParser.ParseTemplate(templateFilePath);
            var parameters = TemplateParser.ParseParameters(parametersFilePath);
            var sortedResources = ResourceDependencies.DependencySort(template.resources, template.existingResources).Reverse().ToList();

            // Create a sequential list of tasks we need to execute.
            var tasks = new List<ITask>();

            if ((template.existingResources != null) && (template.existingResources.Count > 0))
            {
                tasks.Add(new LoadExistingResourcesTask(_accountDetails, _logProvider, template.existingResources));
            }

            foreach (var resource in sortedResources)
            {
                tasks.Add(new DeployResourceTask(_accountDetails, _logProvider, resource));

                if ((resource.scripts != null) && (resource.resourceType == ResourceType.Server))
                {
                    tasks.Add(new ExecuteScriptTask(_logProvider, resource));
                }
            }

            if (template.orchestration != null)
            {
                tasks.Add(new RunOrchestrationTask(_logProvider, template.orchestration, sortedResources));
            }

            // Create the task execution context.
            var context = new TaskContext
            {
                ScriptPath = new FileInfo(templateFilePath).DirectoryName,
                Parameters = parameters,
                ResourcesProperties = new Dictionary<string, JObject>(),
                Log = new DeploymentLog()
                {
                    deploymentTime = DateTime.Now,
                    region = _accountDetails.Region,
                    templateName = template.metadata.templateName,
                    parameters = parameters,
                    resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(tasks, context);
        }

        /// <summary>
        /// Gets the deletion tasks for the supplied deployment log.
        /// </summary>
        /// <param name="deploymentLogFilePath">The deployment log file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor GetDeletionTasks(string deploymentLogFilePath)
        {
            // Create a sequential list of tasks we need to execute.
            var deploymentLog = TemplateParser.ParseDeploymentLog(deploymentLogFilePath);
            var reversedResources = new List<ResourceLog>(deploymentLog.resources);
            reversedResources.Reverse();

            var tasks = reversedResources
                .Where(resource => resource.details != null)
                .Select(resource => (ITask)new DeleteResourceTask(_accountDetails, _logProvider, resource))
                .ToList();

            // Create the task execution context.
            var context = new TaskContext
            {
                Log = new DeploymentLog()
                {
                    deploymentTime = DateTime.Now,
                    region = _accountDetails.Region,
                    resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(tasks, context);
        }
    }
}
