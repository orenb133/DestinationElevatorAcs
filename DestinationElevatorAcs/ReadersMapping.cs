using System.Linq;

namespace DestinationElevatorAcs
{
    internal class ReadersMapping : System.Configuration.ConfigurationSection
    {
        [System.Configuration.ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]

        public ReadersMappingCollection Instances
        {
            get { return (ReadersMappingCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    internal class ReadersMappingCollection : System.Configuration.ConfigurationElementCollection
    {
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new ReadersMappingElement();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((ReadersMappingElement)element).ReaderId;
        }

        public new ReadersMappingElement this[string elementName]
        {
            get{ return this.OfType<ReadersMappingElement>().FirstOrDefault(item => item.ReaderId == elementName); }
        }
    }

    internal class ReadersMappingElement : System.Configuration.ConfigurationElement
    {
        [System.Configuration.ConfigurationProperty("readerId", IsKey = true, IsRequired = true)]
        public string ReaderId
        {
            get { return (string)base["readerId"]; }
            set { base["readerId"] = value; }
        }

        [System.Configuration.ConfigurationProperty("panelId", IsRequired = true)]
        public string PanelId
        {
            get { return (string)base["panelId"]; }
            set { base["panelId"] = value; }
        }
    }
}
