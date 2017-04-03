using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.PriceBuilders
{
    public interface IRetailPriceBuilder
    {
        float GetRetailPrice(PriceItem priceItem);
    }
}
