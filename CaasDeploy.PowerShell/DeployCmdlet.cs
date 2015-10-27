using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library;
using DD.CBU.CaasDeploy.Library.Models;
using Newtonsoft.Json;

namespace DD.CBU.CaasDeploy.PowerShell
{
    /// <summary>
    /// A PowerShell commandlet to deploy a template.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "CaasDeployment")]
    public class DeployCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the template file.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path to the template file that defines the deployment.",
            Position = 0)]
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets the path to the parameters file.
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "The path to the parameters file to use with the deployment.",
            Position = 1)]
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the path to the deployment log file.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path that the deployment log should be written to.",
            Position = 2)]
        public string DeploymentLog { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The region that the template should be deployed to.",
            Position = 3)]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS username for authentication.",
            Position = 4)]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS password for authentication.",
            Position = 5)]
        public string Password { get; set; }

        /// <summary>
        /// Begins the processing.
        /// </summary>
        protected override void BeginProcessing()
        {
            var task = Task.Run(() => BeginProcessingAsync());
            task.Wait();
            base.BeginProcessing();
        }

        /// <summary>
        /// Begins the processing asynchronously.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task BeginProcessingAsync()
        {
            var accountDetails = await CaasAuthentication.Authenticate(UserName, Password, Region);
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider());
            var taskExecutor = taskBuilder.BuildTasksFromDeploymentTemplate(ResolvePath(Template), ResolvePath(Parameters));
            var log = await taskExecutor.Execute(accountDetails);

            Console.WriteLine($"Result: {log.Status}");
            WriteLog(log, ResolvePath(DeploymentLog));
            Console.WriteLine($"Complete! Deployment log written to {DeploymentLog}.");
        }

        /// <summary>
        /// Writes an entry to the log file.
        /// </summary>
        /// <param name="log">The log entry.</param>
        /// <param name="logFile">The log file.</param>
        private static void WriteLog(DeploymentLog log, string logFile)
        {
            using (var sw = new StreamWriter(logFile))
            {
                var json = JsonConvert.SerializeObject(log, Formatting.Indented);
                sw.Write(json);
            }
        }

        /// <summary>
        /// Resolves the supplied file path.
        /// </summary>
        /// <param name="path">The relative file path.</param>
        /// <returns>The absolute file path.</returns>
        private string ResolvePath(string path)
        {
            var workingDir = this.CurrentProviderLocation("FileSystem").Path;
            return Path.Combine(workingDir, path);
        }
    }
}
