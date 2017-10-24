using System.Collections.Generic;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface ICategoryProcessor
    {
        Task<List<category>> GetCategories(PriceItem priceItem);
    }
}