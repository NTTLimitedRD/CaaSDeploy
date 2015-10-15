using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using CaasDeploy.Library.Contracts;
using CaasDeploy.Library.Models;
using CaasDeploy.Library.Utilities;
using CaasDeploy.PostDeployScriptRunner;
using Newtonsoft.Json.Linq;

namespace CaasDeploy.Library.Tasks
{
    /// <summary>
    /// An implemetation of <see cref="ITask"/> which executes a post deployment script.
    /// </summary>
    internal sealed class ExecuteScriptTask : ITask
    {
        private readonly ILogProvider _logProvider;
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
            var details = context.ResourcesProperties[_resource.resourceId];

            _logProvider.LogMessage($"Running deployment scripts.");
            string ipv6Address = details["networkInfo"]["primaryNic"]["ipv6"].Value<string>();
            string ipv6Unc = IPv6ToUnc(ipv6Address);
            OSType osType = details["operatingSystem"]["family"].Value<string>() == "WINDOWS" ? OSType.Windows : OSType.Linux;

            string userName = osType == OSType.Windows ? "administrator" : "root";
            string password = _resource.resourceDefinition["administratorPassword"].Value<string>();

            var scriptRunner = PostDeployScriptRunnerFactory.Create(ipv6Unc, userName, password, osType);

            string scriptPath = UnzipScriptBundle(context.ScriptPath, _resource.scripts.bundleFile);
            var scriptDirectory = new DirectoryInfo(scriptPath);
            foreach (var scriptFile in scriptDirectory.EnumerateFiles())
            {
                _logProvider.LogMessage("\tCopying file " + scriptFile.Name);
                await scriptRunner.UploadScript(scriptFile.FullName);
                scriptFile.Delete();
            }
            scriptDirectory.Delete();

            string deployScript = TokenHelper.SubstitutePropertyTokensInString(_resource.scripts.onDeploy, context.Parameters);
            _logProvider.LogMessage("\tExecuting script " + deployScript);
            await scriptRunner.ExecuteScript(deployScript);
        }

        private string UnzipScriptBundle(string scriptPath, string bundleFile)
        {
            var bundlePath = Path.Combine(scriptPath, bundleFile);
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ZipFile.ExtractToDirectory(bundlePath, path);
            return path;
        }

        private string IPv6ToUnc(string ipv6Address)
        {
            return ipv6Address.Replace(':', '-').Replace('%', 's') + ".ipv6-literal.net";
        }
    }
}
