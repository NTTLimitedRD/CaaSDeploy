using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using DD.CBU.CaasDeploy.Library.Contracts;

namespace DD.CBU.CaasDeploy.Library.Config
{
    /// <summary>
    /// Contains the configuration for the Compute API.
    /// </summary>
    public sealed class ComputeConfigurationSection : ConfigurationSection, IComputeConfiguration
    {
        /// <summary>
        /// Gets or sets all configured regions.
        /// </summary>
        [ConfigurationProperty("regions", IsRequired = true)]
        public RegionConfigurationElementCollection Regions
        {
            get { return (RegionConfigurationElementCollection)this["regions"]; }
            set { this["regions"] = (RegionConfigurationElementCollection)value; }
        }

        /// <summary>
        /// Gets all configured regions.
        /// </summary>
        /// <returns>The configured regions</returns>
        public IEnumerable<IRegion> GetRegions()
        {
            return Regions.Cast<IRegion>();
        }
    }
}