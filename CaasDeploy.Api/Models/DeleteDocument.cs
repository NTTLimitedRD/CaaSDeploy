using CaasDeploy.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaasDeploy.Api.Models
{
    public class DeleteDocument
    {
        public CaasAccountDetails accountDetails { get; set; }
        public DeploymentLog deploymentLog { get; set; }
    }
}