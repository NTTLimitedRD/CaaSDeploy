using System.Collections.Generic;

namespace DD.CBU.CaasDeploy.Library.Contracts
{
    /// <summary>
    /// Represents a single configured region.
    /// </summary>
    public interface IRegion
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        string BaseUrl { get; }
    }
}
