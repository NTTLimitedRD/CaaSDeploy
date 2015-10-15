using System.Collections.Generic;
using System.Threading.Tasks;

using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Contracts
{
    public interface IOrchestrationProvider
    {
        Task RunOrchestration(JObject configuration, Dictionary<string, string> parameters, IEnumerable<Resource> resources, Dictionary<string, JObject> resourcesProperties, ILogProvider logProvider);
    }
}
