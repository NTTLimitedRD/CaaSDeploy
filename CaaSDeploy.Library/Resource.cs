using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CaasDeploy.Library
{
    public class Resource
    {
        public string resourceType { get; set; }
        public string resourceId { get; set; }
        public List<string> dependsOn { get; set; }
        public JObject resourceDefinition { get; set; } 

    }
}