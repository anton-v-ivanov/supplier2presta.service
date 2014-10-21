using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed partial class MultiplicatorsElementCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("default", IsRequired = true)]
        public float Default
        {
            get
            {
                var multiplicator = ((float)(this["default"]));
                if (multiplicator < 1 && multiplicator != 0)
                {
                    throw new ConfigurationErrorsException("Multiplicator must be greater than 1");
                }

                return multiplicator;
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
