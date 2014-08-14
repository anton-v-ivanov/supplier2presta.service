using Supplier2Presta.Entities;

namespace Supplier2Presta.PriceItemBuilders
{
    public interface IPriceItemBuilder
    {
        PriceItem Build(string line);
    }
}
