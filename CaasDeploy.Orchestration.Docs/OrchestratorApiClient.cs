using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;

using DD.CBU.CaasDeploy.Orchestration.Docs.OrchestratorWebService;

namespace DD.CBU.CaasDeploy.Orchestration.Docs
{
    /// <summary>
    /// The orchestration API client.
    /// </summary>
    internal class OrchestratorApiClient
    {
        /// <summary>
        /// The service root.
        /// </summary>
        private string _serviceRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestratorApiClient"/> class.
        /// </summary>
        /// <param name="serviceRoot">The service root.</param>
        public OrchestratorApiClient(string serviceRoot)
        {
            _serviceRoot = serviceRoot;
        }

        /// <summary>
        /// Starts the runbook with parameters.
        /// </summary>
        /// <param name="runbookId">The runbook identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The identifier of the process.</returns>
        public Guid StartRunbookWithParameters(Guid runbookId, IDictionary<string, string> parameters)
        {
            // Create Orchestrator context
            var context = new OrchestratorContext(new Uri(_serviceRoot));
            context.Credentials = new NetworkCredential("administrator", "Password@1");

            // Retrieve parameters for the runbook
            var runbookParams = context.RunbookParameters.Where(runbookParam => runbookParam.RunbookId == runbookId && runbookParam.Direction == "In");

            // Configure the XML for the parameters
            var parametersXml = new StringBuilder();
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
                Job job = new Job
                {
                    RunbookId = runbookId,
                    Parameters = parametersXml.ToString(),
                    CreationTime = DateTime.UtcNow,
                    LastModifiedTime = DateTime.UtcNow
                };

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
