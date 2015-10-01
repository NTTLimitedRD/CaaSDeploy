using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CaasDeploy.Library.Models
{
    public class DeploymentTemplate
    {
        public DeploymentTemplateMetadata metadata { get; set; }
        public JObject parameters { get; set; }
        public List<Resource> resources { get; set; }
        public JObject orchestration { get; set; }
    }
}