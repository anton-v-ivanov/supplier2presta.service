using Supplier2Presta.Entities;

namespace Supplier2Presta.PriceItemBuilders
{
    public interface IPriceItemBuilder<T>
    {
        PriceItem Build(T fileItem);
    }
}
