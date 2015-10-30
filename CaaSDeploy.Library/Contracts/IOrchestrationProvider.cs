using System.Collections.Generic;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Implementations of this interface provide post-deploy orchestration capabilities.
    /// </summary>
    public interface IOrchestrationProvider
    {
        /// <summary>
        /// Runs the orchestration.
        /// </summary>
        /// <param name="runtimeContext">The runtime context.</param>
        /// <param name="taskContext">The task execution context.</param>
        /// <param name="orchestrationObject">The orchestration object.</param>
        /// <param name="resources">The resources.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        Task RunOrchestration(
            RuntimeContext runtimeContext,
            TaskContext taskContext,
            JObject orchestrationObject,
            IEnumerable<Resource> resources);
    }
}
