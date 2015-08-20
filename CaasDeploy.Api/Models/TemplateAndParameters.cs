using CaasDeploy.Library;
using CaasDeploy.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaasDeploy.Api.Models
{
    public class TemplateAndParameters
    {
        public CaasAccountDetails accountDetails { get; set; }
        public DeploymentTemplate template { get; set; }
        public Dictionary<string, string> parameterValues { get; set; }
    }
}