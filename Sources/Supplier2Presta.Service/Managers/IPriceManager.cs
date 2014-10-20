using Supplier2Presta.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Managers
{
    public interface IPriceManager
    {
        PriceUpdateResult CheckProductsUpdates(string priceUrl);

        PriceUpdateResult CheckNewProducts(string priceUrl);
    }
}
