using CaasDeploy.Library.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public interface IOrchestrationProvider
    {
        Task RunOrchestration(JObject configuration, Dictionary<string, string> parameters, IEnumerable<Resource> resources, Dictionary<string, JObject> resourcesProperties, ILogProvider logProvider);
    }
}
