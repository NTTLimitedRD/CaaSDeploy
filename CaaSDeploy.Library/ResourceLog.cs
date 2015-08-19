using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    public class ResourceLog
    {
        public string resourceType { get; set; }
        public string resourceId { get; set; }
        public JObject details { get; set; }
        public JObject error { get; set; }
    }
}
