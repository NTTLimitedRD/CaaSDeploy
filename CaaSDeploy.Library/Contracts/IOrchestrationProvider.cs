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
        /// <param name="configuration">The configuration.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="resourcesProperties">The resources properties.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        Task RunOrchestration(
            JObject configuration,
            IDictionary<string, string> parameters,
            IEnumerable<Resource> resources,
            IDictionary<string, JObject> resourcesProperties,
            ILogProvider logProvider);
    }
}
