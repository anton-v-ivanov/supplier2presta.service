﻿using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface IManufacturerProcessor
    {
        Task<manufacturer> GetManufacturerValue(PriceItem priceItem, product product);
    }
}