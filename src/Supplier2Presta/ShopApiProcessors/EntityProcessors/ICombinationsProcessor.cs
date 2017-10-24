using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface ICombinationsProcessor
    {
        Task<combination> GetOrCreateCombination(product product, Assort assort);
        Task FillOptions(PriceItem priceItem, product product);
    }
}