﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using DD.CBU.CaasDeploy.Library.Contracts;
using DD.CBU.CaasDeploy.Library.Models;
using DD.CBU.CaasDeploy.Library.Utilities;
using DD.CBU.CaasDeploy.PostDeployScriptRunner;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITask"/> which executes a post deployment script.
    /// </summary>
    internal sealed class ExecuteScriptTask : ITask
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The resource
        /// </summary>
        private readonly Resource _resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteScriptTask"/> class.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="resource">The resource.</param>
        public ExecuteScriptTask(ILogProvider logProvider, Resource resource)
        {
            if (logProvider == null)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _logProvider = logProvider;
            _resource = resource;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="context">The task execution context.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task Execute(TaskContext context)
        {
            var details = context.ResourcesProperties[_resource.ResourceId];

            _logProvider.LogMessage($"Running deployment scripts.");
            string ipv6Address = details["networkInfo"]["primaryNic"]["ipv6"].Value<string>();
            string ipv6Unc = IPv6ToUnc(ipv6Address);
            OSType osType = details["operatingSystem"]["family"].Value<string>() == "WINDOWS" ? OSType.Windows : OSType.Linux;

            string userName = osType == OSType.Windows ? "administrator" : "root";
            string password = _resource.ResourceDefinition["administratorPassword"].Value<string>();

            var scriptRunner = PostDeployScriptRunnerFactory.Create(ipv6Unc, userName, password, osType);

            string scriptPath = UnzipScriptBundle(context.ScriptPath, _resource.Scripts.BundleFile);
            var scriptDirectory = new DirectoryInfo(scriptPath);
            foreach (var scriptFile in scriptDirectory.EnumerateFiles())
            {
                _logProvider.LogMessage("\tCopying file " + scriptFile.Name);
                await scriptRunner.UploadScript(scriptFile.FullName);
                scriptFile.Delete();
            }
            scriptDirectory.Delete();

            string deployScript = TokenHelper.SubstitutePropertyTokensInString(_resource.Scripts.OnDeploy, context.Parameters);
            _logProvider.LogMessage("\tExecuting script " + deployScript);
            await scriptRunner.ExecuteScript(deployScript);
        }

        /// <summary>
        /// Unzips the script bundle.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        /// <param name="bundleFile">The bundle file name.</param>
        /// <returns>The absolute path to the extracted folder.</returns>
        private string UnzipScriptBundle(string scriptPath, string bundleFile)
        {
            var bundlePath = Path.Combine(scriptPath, bundleFile);
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ZipFile.ExtractToDirectory(bundlePath, path);
            return path;
        }

        /// <summary>
        /// Converts an IP V6 address to a UNC path.
        /// </summary>
        /// <param name="ipv6Address">The IP V6 address.</param>
        /// <returns>The UNC path</returns>
        private string IPv6ToUnc(string ipv6Address)
        {
            return ipv6Address.Replace(':', '-').Replace('%', 's') + ".ipv6-literal.net";
        }
    }
}
