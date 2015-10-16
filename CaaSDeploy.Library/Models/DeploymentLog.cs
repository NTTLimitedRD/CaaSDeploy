using System;
using System.Collections.Generic;

using CaasDeploy.Library.Utilities;
using Newtonsoft.Json;

namespace CaasDeploy.Library.Models
{
    public class DeploymentLog
    {
        [JsonConverter(typeof(JsonEnumConverter))]
        public DeploymentLogStatus status { get; set; }

        public DateTime deploymentTime { get; set; }

        public string templateName { get; set; }

        public List<ResourceLog> resources { get; set; }
    }
}
