using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace CaasDeploy.Library.Models
{
    public class ExistingResource
    {
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceType resourceType { get; set; }
        public string resourceId { get; set; }
        public string caasId { get; set; }
    }
}
