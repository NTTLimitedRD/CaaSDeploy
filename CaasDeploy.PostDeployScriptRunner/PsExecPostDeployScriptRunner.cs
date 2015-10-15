using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using CaasDeploy.PostDeployScriptRunner.Utilities;

namespace CaasDeploy.PostDeployScriptRunner
{
    /// <summary>
    /// Runs post-deploy scripts on a remote Windows server using PsExec.
    /// </summary>
    public sealed class PsExecPostDeployScriptRunner : IPostDeployScriptRunner
    {
        private string _serverIP;
        private string _userName;
        private string _password;

        private bool _serverReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="PsExecPostDeployScriptRunner"/> class.
        /// </summary>
        /// <param name="serverIP">The server ip.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public PsExecPostDeployScriptRunner(string serverIP, string userName, string password)
        {
            _serverIP = serverIP;
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Uploads the script to the remote server.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        public async Task UploadScript(string localPath)
        {
            await WaitForServerToBeReady();

            await Task.Run(() =>
            {
                var creds = new NetworkCredential(_userName, _password);
                using (new NetworkConnection(@"\\" + _serverIP + @"\admin$", creds))
                {
                    File.Copy(localPath, @"\\" + _serverIP + @"\admin$\" + Path.GetFileName(localPath), true);
                }
            });
        }

        /// <summary>
        /// Executes the script on the remote server.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        /// <returns>The async <see cref="Task"/> with the exit code as result.</returns>
        public async Task<int> ExecuteScript(string commandLine)
        {
            await WaitForServerToBeReady();

            return await Task.Run<int>(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "psexec.exe",
                    Arguments = $"\\\\{_serverIP} -u {_userName} -p {_password} cmd.exe /c " + commandLine,
                };

                var process = Process.Start(startInfo);
                process.WaitForExit();
                return process.ExitCode;
            });
        }

        /// <summary>
        /// Waits for server to be ready.
        /// </summary>
        /// <returns>The async <see cref="Task"/>.</returns>
        private async Task WaitForServerToBeReady()
        {
            await Task.Run(() =>
            {
                if (_serverReady)
                {
                    return;
                }

                _serverReady = true;
            });
        }
    }
}
