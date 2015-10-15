using System.Collections.Generic;

namespace CaasDeploy.Library.Contracts
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

    /// <summary>
    /// Contains the configuration for the Compute API.
    /// </summary>
    public interface IComputeConfiguration
    {
        /// <summary>
        /// Gets all configured regions.
        /// </summary>
        /// <returns>The configured regions</returns>
        IEnumerable<IRegion> GetRegions();
    }
}
