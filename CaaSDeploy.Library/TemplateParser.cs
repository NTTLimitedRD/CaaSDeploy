using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Tasks;
using DD.CBU.CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library
{
    /// <summary>
    /// Builds task lists and contexts from deployment template documents.
    /// </summary>
    public sealed class TemplateParser : ITemplateParser
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateParser"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        public TemplateParser(ILogProvider logProvider)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            _logProvider = logProvider;
        }

        /// <summary>
        /// Gets the deployment tasks for the supplied deployment template.
        /// </summary>
        /// <param name="accountDetails">The CaaS account details.</param>
        /// <param name="templateFilePath">The template file path.</param>
        /// <param name="parametersFilePath">The parameters file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor ParseDeploymentTemplate(CaasAccountDetails accountDetails, string templateFilePath, string parametersFilePath)
        {
            var template = ParseTemplate(templateFilePath);
            var parameters = ParseParameters(parametersFilePath);
            var sortedResources = ResourceDependencies.DependencySort(template.Resources, template.ExistingResources).Reverse().ToList();

            // Create a sequential list of tasks we need to execute.
            var tasks = new List<ITask>();

            if ((template.ExistingResources != null) && (template.ExistingResources.Count > 0))
            {
                tasks.Add(new LoadExistingResourcesTask(_logProvider, template.ExistingResources));
            }

            foreach (var resource in sortedResources)
            {
                tasks.Add(new DeployResourceTask(_logProvider, resource));

                if ((resource.Scripts != null) && (resource.ResourceType == ResourceType.Server))
                {
                    tasks.Add(new ExecuteScriptTask(_logProvider, resource));
                }
            }

            if (template.Orchestration != null)
            {
                tasks.Add(new RunOrchestrationTask(_logProvider, template.Orchestration, sortedResources));
            }

            // Create the task execution context.
            var context = new TaskContext
            {
                AccountDetails = accountDetails,
                ScriptPath = new FileInfo(templateFilePath).DirectoryName,
                Parameters = parameters,
                ResourcesProperties = new Dictionary<string, JObject>(),
                Log = new DeploymentLog()
                {
                    DeploymentTime = DateTime.Now,
                    TemplateName = template.Metadata.TemplateName,
                    Resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(tasks, context);
        }

        /// <summary>
        /// Gets the deletion tasks for the supplied deployment log.
        /// </summary>
        /// <param name="accountDetails">The CaaS account details.</param>
        /// <param name="deploymentLogFilePath">The deployment log file path.</param>
        /// <returns>Instance of <see cref="TaskExecutor"/> with tasks and task execution context.</returns>
        public TaskExecutor ParseDeploymentLog(CaasAccountDetails accountDetails, string deploymentLogFilePath)
        {
            // Create a sequential list of tasks we need to execute.
            var deploymentLog = ParseLog(deploymentLogFilePath);
            var reversedResources = new List<ResourceLog>(deploymentLog.Resources);
            reversedResources.Reverse();

            var tasks = reversedResources
                .Where(resource => resource.CaasId != null)
                .Select(resource => (ITask)new DeleteResourceTask(_logProvider, resource))
                .ToList();

            // Create the task execution context.
            var context = new TaskContext
            {
                AccountDetails = accountDetails,
                Log = new DeploymentLog()
                {
                    DeploymentTime = DateTime.Now,
                    TemplateName = deploymentLog.TemplateName,
                    Resources = new List<ResourceLog>()
                }
            };

            return new TaskExecutor(tasks, context);
        }

        /// <summary>
        /// Parses the template.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed deployment template.</returns>
        private DeploymentTemplate ParseTemplate(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentTemplate>(content);
            }
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed parameters.</returns>
        private Dictionary<string, string> ParseParameters(string fileName)
        {
            var dict = new Dictionary<string, string>();
            if (fileName == null)
            {
                return dict;
            }

            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                var jObject = JObject.Parse(content);
                foreach (var param in ((JObject)jObject["parameters"]).Properties())
                {
                    dict.Add(param.Name, param.Value["value"].Value<string>());
                }
                return dict;
            }
        }

        /// <summary>
        /// Parses a deployment log file.
        /// </summary>
        /// <param name="fileName">Path to the file.</param>
        /// <returns>The parsed deployment log.</returns>
        private DeploymentLog ParseLog(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                var content = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<DeploymentLog>(content);
            }
        }
    }
}
