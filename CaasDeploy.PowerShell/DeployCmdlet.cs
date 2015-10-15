﻿using System;
using System.Configuration;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

using CaasDeploy.Library;
using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using Newtonsoft.Json;

namespace CaasDeploy.PowerShell
{
    [Cmdlet(VerbsCommon.New, "CaasDeployment")]
    public class DeployCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path to the template file that defines the deployment.",
            Position = 0
            )]
        public string Template { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The path to the parameters file to use with the deployment.",
            Position = 1
            )]
        public string Parameters { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The path that the deployment log should be written to.",
            Position = 2
            )]
        public string DeploymentLog { get; set; }


        [Parameter(
            Mandatory = true,
            HelpMessage = "The region that the template should be deployed to.",
            Position = 3
            )]
        public string Region { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS username for authentication.",
            Position = 4
            )]
        public string UserName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The CaaS password for authentication.",
            Position = 5
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
            var taskBuilder = new TaskBuilder(new ConsoleLogProvider(), accountDetails);
            var taskExecutor = taskBuilder.GetDeploymentTasks(ResolvePath(Template), ResolvePath(Parameters));
            var log = await taskExecutor.Execute();

            Console.WriteLine($"Result: {log.status}");
            WriteLog(log, ResolvePath(DeploymentLog));
            Console.WriteLine($"Complete! Deployment log written to {DeploymentLog}.");
        }

        private string ResolvePath(string path)
        {
            var workingDir = this.CurrentProviderLocation("FileSystem").Path;
            return Path.Combine(workingDir, path);
        }

        private static void WriteLog(DeploymentLog log, string logFile)
        {
            using (var sw = new StreamWriter(logFile))
            {
                var json = JsonConvert.SerializeObject(log, Formatting.Indented);
                sw.Write(json);
            }
        }
    }
}
