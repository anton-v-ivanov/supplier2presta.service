using Supplier2Presta.Service.Entities.XmlPrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public interface IPriceLoader
    {
        PriceLoadResult Load<T>(string uri);
    }
}
