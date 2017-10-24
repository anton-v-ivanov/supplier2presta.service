using Supplier2Presta.Entities;

namespace Supplier2Presta.PriceBuilders
{
    public interface IRetailPriceBuilder
    {
        float GetRetailPrice(PriceItem priceItem);
    }
}
