using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library.Models
{
    public class ExistingResource
    {
        public string resourceType { get; set; }
        public string resourceId { get; set; }
        public string caasId { get; set; }
    }
}
