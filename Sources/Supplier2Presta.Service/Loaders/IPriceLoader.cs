using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public interface IPriceLoader
    {
        PriceLoadResult Load(string uri, string encoding);
    }
}
