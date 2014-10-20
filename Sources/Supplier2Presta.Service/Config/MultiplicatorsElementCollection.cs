using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed partial class MultiplicatorsElementCollection : ConfigurationElementCollection
    {
        [ConfigurationPropertyAttribute("default", IsRequired = true)]
        public float Default
        {
            get
            {
                return ((float)(this["default"]));
            }
            set
            {
                this["default"] = value;
            }
        }

        public MultiplicatorRuleElement this[int i]
        {
            get
            {
                return ((MultiplicatorRuleElement)(this.BaseGet(i)));
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MultiplicatorRuleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MultiplicatorRuleElement)(element)).Name;
        }
    }
}
