using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Models
{
    public class DeploymentLog
    {
        public string status { get; set; }
        public DateTime deploymentTime { get; set; }
        public string region { get; set; }
        public string templateName { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public List<ResourceLog> resources { get; set; }
    }
}
