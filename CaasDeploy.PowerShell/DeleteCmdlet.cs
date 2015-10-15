using System;
using System.Configuration;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

using CaasDeploy.Library;
using CaasDeploy.Library.Contracts;

namespace CaasDeploy.PowerShell
{
    [Cmdlet(VerbsCommon.Remove, "CaasDeployment")]
    public class DeleteCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path to the deployment log file for the deployment that should be deleted.",
            Position = 0
            )]
        public string DeploymentLog { get; set; }


        [Parameter(
            Mandatory = true,
            HelpMessage = "The region that the template is deployed to.",
            Position = 1
            )]
        public string Region { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS username for authentication.",
            Position = 2
            )]
        public string UserName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS password for authentication.",
            Position = 3
            )]
        public string Password { get; set; }

        protected override void BeginProcessing()
        {
            var task = Task.Run(() => BeginProcessingAsync());
            task.Wait();
            base.BeginProcessing();
        }

        private async Task BeginProcessingAsync()
        {
            var config = (IComputeConfiguration)ConfigurationManager.GetSection("compute");
            var accountDetails = await CaasAuthentication.Authenticate(config, UserName, Password, Region);
            var d = new TaskBuilder(new ConsoleLogProvider(), accountDetails);

            var taskExecutor = d.GetDeletionTasks(ResolvePath(DeploymentLog));
            var log = await taskExecutor.Execute();

            Console.WriteLine($"Result: {log.status}");
        }

        private string ResolvePath(string path)
        {
            var workingDir = this.CurrentProviderLocation("FileSystem").Path;
            return Path.Combine(workingDir, path);
        }
    }
}
