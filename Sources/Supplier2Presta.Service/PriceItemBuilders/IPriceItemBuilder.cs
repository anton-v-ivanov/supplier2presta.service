using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public interface IPriceItemBuilder
    {
        PriceItem Build(string line);
    }
}
