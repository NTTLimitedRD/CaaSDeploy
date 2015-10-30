using DD.CBU.CaasDeploy.Library.Models;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// The runtime context provides dependent instances across tasks.
    /// </summary>
    public sealed class RuntimeContext
    {
        /// <summary>
        /// Gets or sets the account details.
        /// </summary>
        public CaasAccountDetails AccountDetails { get; set; }

        /// <summary>
        /// Gets or sets the log provider.
        /// </summary>
        public ILogProvider LogProvider { get; set; }
    }
}
