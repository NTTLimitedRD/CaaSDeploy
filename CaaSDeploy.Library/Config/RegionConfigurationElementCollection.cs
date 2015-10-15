using System.Configuration;

namespace CaasDeploy.Library.Config
{
    public sealed class RegionConfigurationElementCollection : ConfigurationElementCollection
    {
        public RegionConfigurationElementCollection()
        {
        }

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

        public void Add(RegionConfigurationElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RegionConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RegionConfigurationElement)element).Key;
        }

        public void Remove(RegionConfigurationElement serviceConfig)
        {
            BaseRemove(serviceConfig.Key);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}