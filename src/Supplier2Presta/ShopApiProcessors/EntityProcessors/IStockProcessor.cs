using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface IStockProcessor
    {
        Task UpdateStockValue(PriceItem priceItem, product product);
    }
}