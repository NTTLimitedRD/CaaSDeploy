using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tasks;
using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Builds task lists and contexts from deployment templates or logs.
    /// </summary>
    public sealed class TaskBuilder
    {
        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="templateFilePath">The template file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasksFromDeploymentTemplate(string templateFilePath)
        {
            var template = TemplateParser.ParseDeploymentTemplate(templateFilePath);
            return BuildTasks(template, new FileInfo(templateFilePath).DirectoryName, new Dictionary<string, string>());
        }

        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="parametersFilePath">The parameters file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasksFromDeploymentTemplate(string templateFilePath, string parametersFilePath)
        {
            var template = TemplateParser.ParseDeploymentTemplate(templateFilePath);
            var parameters = TemplateParser.ParseDeploymentParameters(parametersFilePath);
            return BuildTasks(template, new FileInfo(templateFilePath).DirectoryName, parameters);
        }

        /// <summary>
        /// Gets the deletion tasks for the supplied deployment log.
        /// </summary>
        /// <param name="deploymentLogFilePath">The deployment log file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasksFromDeploymentLog(string deploymentLogFilePath)
        {
            var deploymentLog = TemplateParser.ParseDeploymentLog(deploymentLogFilePath);
            return BuildTasks(deploymentLog);
        }

        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="template">The deployment template.</param>
        /// <param name="scriptPath">The script path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasks(DeploymentTemplate template, string scriptPath)
        {
            return BuildTasks(template, scriptPath, new Dictionary<string, string>());
        }

        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="template">The deployment template.</param>
        /// <param name="scriptPath">The script path.</param>
        /// <param name="parameters">The deployment parameters.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasks(DeploymentTemplate template, string scriptPath, IDictionary<string, string> parameters)
        {
            var sortedResources = ResourceDependencies.DependencySort(template.Resources, template.ExistingResources).Reverse().ToList();

            // Create a sequential list of tasks we need to execute.
            var tasks = new List<ITask>();

            if ((template.ExistingResources != null) && (template.ExistingResources.Count > 0))
            {
                tasks.Add(new LoadExistingResourcesTask(template.ExistingResources));
            }

            foreach (var resource in sortedResources)
            {
                tasks.Add(new DeployResourceTask(resource));

                if ((resource.Scripts != null) && (resource.ResourceType == ResourceType.Server))
                {
                    tasks.Add(new ExecuteScriptTask(resource));
                }
            }

            if (template.Orchestration != null)
            {
                tasks.Add(new RunOrchestrationTask(template.Orchestration, sortedResources));
            }

            // Create the task execution context.
            var context = new TaskContext
            {
                ScriptPath = scriptPath,
                Parameters = parameters,
                ResourcesProperties = new Dictionary<string, JObject>(),
                Log = new DeploymentLog()
                {
                    DeploymentTime = DateTime.Now,
                    TemplateName = template.Metadata.TemplateName,
                    Resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(template, tasks, context);
        }

        /// <summary>
        /// Gets the deletion tasks for the supplied deployment log.
        /// </summary>
        /// <param name="deploymentLog">The deployment log.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor BuildTasks(DeploymentLog deploymentLog)
        {
            // Create a sequential list of tasks we need to execute.
            var reversedResources = new List<ResourceLog>(deploymentLog.Resources);
            reversedResources.Reverse();

            var tasks = reversedResources
                .Where(resource => resource.CaasId != null)
                .Select(resource => (ITask)new DeleteResourceTask(resource))
                .ToList();

            // Create the task execution context.
            var context = new TaskContext
            {
                Log = new DeploymentLog()
                {
                    DeploymentTime = DateTime.Now,
                    TemplateName = deploymentLog.TemplateName,
                    Resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(null, tasks, context);
        }
    }
}
