using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class DeploymentLog
    {
        public DateTime deploymentTime { get; set; }
        public string region { get; set; }
        public string templateFile { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public List<ResourceLog> resources { get; set; }
    }
}
