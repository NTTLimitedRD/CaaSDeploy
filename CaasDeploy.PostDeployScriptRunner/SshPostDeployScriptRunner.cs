using System;
using System.Threading.Tasks;

namespace DD.CBU.CaasDeploy.PostDeployScriptRunner
{
    /// <summary>
    /// Runs post-deploy scripts on a remote Linux server using SSH.
    /// </summary>
    public sealed class SshPostDeployScriptRunner : IPostDeployScriptRunner
    {
        /// <summary>
        /// The server IP
        /// </summary>
        private readonly string _serverIP;

        /// <summary>
        /// The user name
        /// </summary>
        private readonly string _userName;

        /// <summary>
        /// The password
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SshPostDeployScriptRunner"/> class.
        /// </summary>
        /// <param name="serverIP">The server IP.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public SshPostDeployScriptRunner(string serverIP, string userName, string password)
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
            await Task.FromResult((object)null);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the script on the remote server.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        /// <returns>The async <see cref="Task"/> with the exit code as result.</returns>
        public async Task<int> ExecuteScript(string commandLine)
        {
            await Task.FromResult((object)null);
            throw new NotImplementedException();
        }
    }
}
