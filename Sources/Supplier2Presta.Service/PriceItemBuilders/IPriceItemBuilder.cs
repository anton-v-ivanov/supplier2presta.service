using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public interface IPriceItemBuilder<T>
    {
        PriceItem Build(T fileItem);
    }
}
