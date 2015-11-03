using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which executes an orchestration.
    /// </summary>
    public sealed class RunOrchestrationTask : ITask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunOrchestrationTask"/> class.
        /// </summary>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="resources">The resources.</param>
        public RunOrchestrationTask(JObject orchestration, IReadOnlyList<Resource> resources)
        {
            if (orchestration == null)
            {
                throw new ArgumentNullException(nameof(orchestration));
            }

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            Orchestration = orchestration;
            Resources = resources;
        }

        /// <summary>
        /// Gets the orchestration.
        /// </summary>
        public JObject Orchestration { get; private set; }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        public IReadOnlyList<Resource> Resources { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(RuntimeContext runtimeContext, TaskContext taskContext)
        {
            var providerTypeName = Orchestration["provider"].Value<String>();
            var providerType = Type.GetType(providerTypeName);
            if (providerType == null)
            {
                runtimeContext.LogProvider.LogError($"Unable to create Orchestration Provider of type {providerTypeName}.");
                return;
            }

            var provider = (IOrchestrationProvider)Activator.CreateInstance(providerType);
            runtimeContext.LogProvider.LogMessage($"Running Orchestration Provider '{providerTypeName}'.");

            await provider.RunOrchestration(runtimeContext, taskContext, Orchestration, Resources);
        }
    }
}
