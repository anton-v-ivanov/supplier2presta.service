using System.Threading.Tasks;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors
{
    public interface IProductCreator
    {
        Task Create(PriceItem priceItem);
    }
}