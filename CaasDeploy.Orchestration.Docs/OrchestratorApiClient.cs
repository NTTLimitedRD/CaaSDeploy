using CaasDeploy.Orchestration.Docs.OrchestratorWebService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Orchestration.Docs
{
    class OrchestratorApiClient
    {
        public Guid StartRunbookWithParameters(Guid runbookId, Dictionary<string, string> parameters)
        {

            // Path to Orchestrator web service
            string serviceRoot = "http://[2607:f480:111:1175:39ff:74f3:3216:ba41]:81/Orchestrator2012/Orchestrator.svc";

            // Create Orchestrator context
            OrchestratorContext context = new OrchestratorContext(new Uri(serviceRoot));

            // Set credentials to default or a specific user.
            //context.Credentials = System.Net.CredentialCache.DefaultCredentials;
            context.Credentials = new System.Net.NetworkCredential("administrator", "Password@1");

            // Retrieve parameters for the runbook
            var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId && runbookParam.Direction == "In");

            // Configure the XML for the parameters
            StringBuilder parametersXml = new StringBuilder();
            if (runbookParams != null && runbookParams.Count() > 0)
            {
                parametersXml.Append("<Data>");
                foreach (var param in runbookParams)
                {
                    parametersXml.AppendFormat("<Parameter><ID>{0}</ID><Value>{1}</Value></Parameter>", param.Id.ToString("B"), parameters[param.Name]);
                }
                parametersXml.Append("</Data>");
            }

            try
            {
                // Create new job and assign runbook Id and parameters.
                Job job = new Job();
                job.RunbookId = runbookId;
                job.Parameters = parametersXml.ToString();
                job.CreationTime = DateTime.UtcNow;
                job.LastModifiedTime = DateTime.UtcNow;

                // Add newly created job.
                context.AddToJobs(job);
                context.SaveChanges();

                return job.Id;
            }
            catch (DataServiceQueryException ex)
            {
                throw new ApplicationException("Error starting runbook.", ex);
            }
        }
    }

}

