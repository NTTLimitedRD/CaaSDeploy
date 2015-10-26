using System.Collections.Generic;

namespace CaasDeploy.Library.Contracts
{
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
