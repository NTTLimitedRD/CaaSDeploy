using System;

namespace CaasDeploy.PostDeployScriptRunner
{
    /// <summary>
    /// Creates a new instance of <see cref=""/> for the supplied <see cref="OSType"/>.
    /// </summary>
    public static class PostDeployScriptRunnerFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref=""/> for the supplied <see cref="OSType"/>.
        /// </summary>
        /// <param name="serverIP">The server ip.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="osType">Type of the operating system.</param>
        /// <returns>Instance of <see cref="IPostDeployScriptRunner"/> for the supplied operating system.</returns>
        public static IPostDeployScriptRunner Create(string serverIP, string userName, string password, OSType osType)
        {
            switch (osType)
            {
                case OSType.Linux:
                    return new SshPostDeployScriptRunner(serverIP, userName, password);
                case OSType.Windows:
                    return new PsExecPostDeployScriptRunner(serverIP, userName, password);
                default:
                    throw new InvalidOperationException("Unknown OSType enumeration value.");
            }
        }
    }
}
