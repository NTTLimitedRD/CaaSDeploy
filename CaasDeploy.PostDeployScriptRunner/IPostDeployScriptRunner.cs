using System.Threading.Tasks;

namespace CaasDeploy.PostDeployScriptRunner
{
    /// <summary>
    /// Runs post-deploy scripts on a remote server.
    /// </summary>
    public interface IPostDeployScriptRunner
    {
        /// <summary>
        /// Uploads the script to the remote server.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>The async <see cref="Task"/>.</returns>
        Task UploadScript(string localPath);

        /// <summary>
        /// Executes the script on the remote server.
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        /// <returns>The async <see cref="Task"/> with the exit code as result.</returns>
        Task<int> ExecuteScript(string commandLine);
    }
}