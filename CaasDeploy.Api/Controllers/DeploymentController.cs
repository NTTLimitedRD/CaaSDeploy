using CaasDeploy.Api.Models;
using CaasDeploy.Library;
using CaasDeploy.Library.Models;
using Hangfire;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CaasDeploy.Api.Controllers
{
    public class DeploymentController : ApiController
    {
        [HttpPost]
        public string Deploy([FromBody] TemplateAndParameters document)
        {
            var jobId = BackgroundJob.Enqueue<Deployment>(x => x.DeploySync(document.template, document.parameterValues, document.accountDetails));
            return jobId;
        }

        [HttpGet]
        public JobStatus Status(string id)
        {
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
            var details = monitoringApi.JobDetails(id);

            var status = new JobStatus()
            {
                status = details.History[0].StateName,
            };
            if (details.History[0].Data.ContainsKey("Result"))
            {
                status.result = details.History[0].Data["Result"];
            }
            return status;
            
        }

        [HttpDelete]
        public string Delete([FromBody] DeploymentLog deploymentLog)
        {
            throw new NotImplementedException();
        }
    }
}
