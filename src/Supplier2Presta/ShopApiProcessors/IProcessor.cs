using System.Collections.Generic;
using System.Threading.Tasks;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors
{
    public interface IProcessor
    {
        Task Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType, PriceType processingPriceType);
    }
}
