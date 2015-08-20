using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Models
{
    public class DeploymentTemplateMetadata
    {
        public string schemaVersion { get; set; }
        public string templateName { get; set; }
        public string templateDescription { get; set; }
    }
}
