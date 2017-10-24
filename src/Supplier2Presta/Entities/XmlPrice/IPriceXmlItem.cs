using System.Collections.Generic;

namespace Supplier2Presta.Entities.XmlPrice
{
    public interface IPriceXmlItem
    {
        List<XmlItem> Items { get; set; }
    }
}
