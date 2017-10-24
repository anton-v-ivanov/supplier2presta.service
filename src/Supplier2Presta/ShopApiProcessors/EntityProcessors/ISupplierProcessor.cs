using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface ISupplierProcessor
    {
        Task<product_supplier> GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature);
    }
}