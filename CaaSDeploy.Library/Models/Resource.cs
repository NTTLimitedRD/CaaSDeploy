using System.Collections.Generic;

using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Models
{
    public class Resource
    {
        [JsonConverter(typeof(JsonEnumConverter))]
        public ResourceType resourceType { get; set; }

        public string resourceId { get; set; }

        public List<string> dependsOn { get; set; }

        public JObject resourceDefinition { get; set; }

        public Scripts scripts { get; set; }
    }
}