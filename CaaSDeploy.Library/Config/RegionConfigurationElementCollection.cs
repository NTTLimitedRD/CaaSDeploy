using System.Configuration;

namespace DD.CBU.CaasDeploy.Library.Config
{
    /// <summary>
    /// Represents all configured regions.
    /// </summary>
    public sealed class RegionConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionConfigurationElementCollection"/> class.
        /// </summary>
        public RegionConfigurationElementCollection()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="RegionConfigurationElement"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="RegionConfigurationElement"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The element</returns>
        public RegionConfigurationElement this[int index]
        {
            get
            {
                return (RegionConfigurationElement)BaseGet(index);
            }

            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Adds a new element.
        /// </summary>
        /// <param name="serviceConfig">The service configuration.</param>
        public void Add(RegionConfigurationElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Creates the new element.
        /// </summary>
        /// <returns>The new element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RegionConfigurationElement();
        }

        /// <summary>
        /// Gets an element key.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RegionConfigurationElement)element).Key;
        }

        /// <summary>
        /// Removes the supplied element.
        /// </summary>
        /// <param name="serviceConfig">The service configuration.</param>
        public void Remove(RegionConfigurationElement serviceConfig)
        {
            BaseRemove(serviceConfig.Key);
        }

        /// <summary>
        /// Removes an element by its index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes an element by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}