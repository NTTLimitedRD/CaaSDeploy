using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CaasDeploy.Library
{
    public class DeploymentTemplate
    {
        public JObject parameters { get; set; }

        public List<Resource> resources { get; set; }
    }
}