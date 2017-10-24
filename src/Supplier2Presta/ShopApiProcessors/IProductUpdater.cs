using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors
{
    public interface IProductUpdater
    {
        Task Update(product product, PriceItem item, PriceType processingPriceType);
        Task RemoveDiscountInfo(PriceItem item, product product);
    }
}