using CaasDeploy.Api.Models;
using CaasDeploy.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CaasDeploy.Api.Controllers
{
    public class DeploymentController : ApiController
    {
        [HttpPost]
        public string Deploy([FromBody] TemplateAndParameters templateAndParametersDocument)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public JobStatus Status(string jobId)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public string Delete([FromBody] DeploymentLog deploymentLog)
        {
            throw new NotImplementedException();
        }
    }
}
