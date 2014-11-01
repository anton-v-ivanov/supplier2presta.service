using System;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using System.Collections.Generic;

namespace Supplier2Presta.Service.Processors
{
    public interface IProcessor
    {
        void Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType, PriceType processingPriceType);
    }
}
