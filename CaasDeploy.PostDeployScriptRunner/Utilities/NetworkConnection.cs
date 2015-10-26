using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace CaasDeploy.PostDeployScriptRunner.Utilities
{
    /// <summary>
    /// Wrapper for low-level network connection functionality.
    /// </summary>
    internal class NetworkConnection : IDisposable
    {
        /// <summary>
        /// The network name.
        /// </summary>
        private readonly string _networkName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkConnection"/> class.
        /// </summary>
        /// <param name="networkName">Name of the network.</param>
        /// <param name="credentials">The credentials.</param>
        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(netResource, credentials.Password, userName, 0);
            if (result != 0)
            {
                throw new Win32Exception(result, "Error connecting to remote share");
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NetworkConnection"/> class.
        /// </summary>
        ~NetworkConnection()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        /// <summary>
        /// DLL import declaration.
        /// </summary>
        /// <param name="netResource">The net resource.</param>
        /// <param name="password">The password.</param>
        /// <param name="username">The username.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>Result code.</returns>
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        /// <summary>
        /// DLL import declaration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="force">If set to <c>true</c>, the cancellation will be enforced.</param>
        /// <returns>Result code.</returns>
        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);
    }

    /// <summary>
    /// The API network resource structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        /// <summary>
        /// The scope
        /// </summary>
        public ResourceScope Scope;

        /// <summary>
        /// The resource type
        /// </summary>
        public ResourceType ResourceType;

        /// <summary>
        /// The display type
        /// </summary>
        public ResourceDisplaytype DisplayType;

        /// <summary>
        /// The usage
        /// </summary>
        public int Usage;

        /// <summary>
        /// The local name
        /// </summary>
        public string LocalName;

        /// <summary>
        /// The remote name
        /// </summary>
        public string RemoteName;

        /// <summary>
        /// The comment
        /// </summary>
        public string Comment;

        /// <summary>
        /// The provider
        /// </summary>
        public string Provider;
    }

    /// <summary>
    /// The API resource scope enumeration.
    /// </summary>
    public enum ResourceScope : int
    {
        /// <summary>
        /// The connected scope
        /// </summary>
        Connected = 1,

        /// <summary>
        /// The global network scope
        /// </summary>
        GlobalNetwork,

        /// <summary>
        /// The remembered scope
        /// </summary>
        Remembered,

        /// <summary>
        /// The recent scope
        /// </summary>
        Recent,

        /// <summary>
        /// The context scope
        /// </summary>
        Context
    }

    /// <summary>
    /// The API resource type enumeration.
    /// </summary>
    public enum ResourceType : int
    {
        /// <summary>
        /// Any type
        /// </summary>
        Any = 0,

        /// <summary>
        /// The disk type
        /// </summary>
        Disk = 1,

        /// <summary>
        /// The print type
        /// </summary>
        Print = 2,

        /// <summary>
        /// The reserved type
        /// </summary>
        Reserved = 8
    }

    /// <summary>
    /// The API resource display type enumeration.
    /// </summary>
    public enum ResourceDisplaytype : int
    {
        /// <summary>
        /// The generic display type
        /// </summary>
        Generic = 0x0,

        /// <summary>
        /// The domain display type
        /// </summary>
        Domain = 0x01,

        /// <summary>
        /// The server display type
        /// </summary>
        Server = 0x02,

        /// <summary>
        /// The share display type
        /// </summary>
        Share = 0x03,

        /// <summary>
        /// The file display type
        /// </summary>
        File = 0x04,

        /// <summary>
        /// The group display type
        /// </summary>
        Group = 0x05,

        /// <summary>
        /// The network display type
        /// </summary>
        Network = 0x06,

        /// <summary>
        /// The root display type
        /// </summary>
        Root = 0x07,

        /// <summary>
        /// The share admin display type
        /// </summary>
        Shareadmin = 0x08,

        /// <summary>
        /// The directory display type
        /// </summary>
        Directory = 0x09,

        /// <summary>
        /// The tree display type
        /// </summary>
        Tree = 0x0a,

        /// <summary>
        /// The NDS container display type
        /// </summary>
        Ndscontainer = 0x0b
    }
}
