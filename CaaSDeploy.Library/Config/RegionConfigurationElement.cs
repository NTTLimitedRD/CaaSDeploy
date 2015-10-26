using System.Configuration;
using DD.CBU.CaasDeploy.Library.Contracts;

namespace DD.CBU.CaasDeploy.Library.Config
{
    /// <summary>
    /// Represents a single configured region.
    /// </summary>
    public sealed class RegionConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionConfigurationElement"/> class.
        /// </summary>
        public RegionConfigurationElement()
        {
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        [ConfigurationProperty("key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        [ConfigurationProperty("baseUrl", IsRequired = true)]
        public string BaseUrl
        {
            get { return (string)this["baseUrl"]; }
            set { this["baseUrl"] = value; }
        }
    }
}