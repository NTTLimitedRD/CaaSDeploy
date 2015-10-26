using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library;

namespace DD.CBU.CaasDeploy.PowerShell
{
    /// <summary>
    /// A PowerShell commandlet to delete a previously deployed template.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "CaasDeployment")]
    public class DeleteCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the deployment log file.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path to the deployment log file for the deployment that should be deleted.",
            Position = 0)]
        public string DeploymentLog { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The region that the template is deployed to.",
            Position = 1)]
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS username for authentication.",
            Position = 2)]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS password for authentication.",
            Position = 3)]
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
            var parser = new DeploymentTemplateParser(new ConsoleLogProvider());
            var taskExecutor = parser.GetDeletionTasks(accountDetails, ResolvePath(DeploymentLog));
            var log = await taskExecutor.Execute();

            Console.WriteLine($"Result: {log.Status}");
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
