using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Models
{
    public class ResourceLog
    {
        public const string DeploymentStatusDeployed = "Deployed";
        public const string DeploymentStatusFailed = "Failed";
        public const string DeploymentStatusAlreadyPresent = "AlreadyPresent";

        public string resourceType { get; set; }
        public string resourceId { get; set; }
        public string deploymentStatus { get; set; }
        public JObject details { get; set; }
        public JObject error { get; set; }
    }
}
