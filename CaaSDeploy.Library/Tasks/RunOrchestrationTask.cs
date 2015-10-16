using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which executes an orchestration.
    /// </summary>
    internal sealed class RunOrchestrationTask : ITask
    {
        private readonly ILogProvider _logProvider;
        private readonly JObject _orchestration;
        private readonly IReadOnlyList<Resource> _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunOrchestrationTask"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="resources">The resources.</param>
        public RunOrchestrationTask(ILogProvider logProvider, JObject orchestration, IReadOnlyList<Resource> resources)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (orchestration == null)
            {
                throw new ArgumentNullException(nameof(orchestration));
            }

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            _logProvider = logProvider;
            _orchestration = orchestration;
            _resources = resources;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(TaskContext context)
        {
            var providerTypeName = _orchestration["provider"].Value<String>();
            var providerType = Type.GetType(providerTypeName);
            if (providerType == null)
            {
                _logProvider.LogError($"Unable to create Orchestration Provider of type {providerTypeName}.");
                return;
            }

            var provider = (IOrchestrationProvider)Activator.CreateInstance(providerType);
            _logProvider.LogMessage($"Running Orchestration Provider '{providerTypeName}'.");

            await provider.RunOrchestration(_orchestration, context.Parameters, _resources, context.ResourcesProperties, _logProvider);
        }
    }
}
