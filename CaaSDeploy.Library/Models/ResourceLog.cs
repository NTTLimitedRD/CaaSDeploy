using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Models
{
    public class ResourceLog
    {
        [JsonConverter(typeof(JsonEnumConverter))]
        public DeploymentStatus deploymentStatus { get; set; }

        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceType resourceType { get; set; }

        public string resourceId { get; set; }

        public string caasId { get; set; }

        [JsonIgnore]
        public JObject details { get; set; }

        public JObject error { get; set; }
    }
}
