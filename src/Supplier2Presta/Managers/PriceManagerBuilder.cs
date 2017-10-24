using System;
using System.IO;
using System.Reflection;
using Supplier2Presta.Config;
using Supplier2Presta.Entities;
using Supplier2Presta.PriceBuilders;
using Supplier2Presta.PriceItemBuilders;
using Supplier2Presta.ShopApiProcessors;

namespace Supplier2Presta.Managers
{
    public class PriceManagerBuilder: IPriceManagerBuilder
    {
        private readonly IColorBuilder _colorBuilder;
        private readonly IProcessor _processor;

        public PriceManagerBuilder(IColorBuilder colorBuilder, IProcessor processor)
        {
            _colorBuilder = colorBuilder;
            _processor = processor;
        }

        public IPriceManager Build(SupplierSettings supplierSettings)
        {
            if(supplierSettings.Supplier != SupplierType.Happiness)
                throw new Exception($"There is no price manager for supplier type {supplierSettings.Supplier}");

            var archiveDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), supplierSettings.ArchiveDirectory);

            IRetailPriceBuilder retailPriceBuilder = new RetailPriceBuilder(supplierSettings);

            switch (Path.GetExtension(supplierSettings.Url))
            {
                case ".xml":
                    return new HappinesXmlPriceManager(_processor, supplierSettings, archiveDirectory, retailPriceBuilder, _colorBuilder);
                
                case ".csv":
                    return new HappinesCsvPriceManager(_processor, supplierSettings, archiveDirectory, retailPriceBuilder);

                default:
                    throw new Exception($"There is no price manager for file with extension {Path.GetExtension(supplierSettings.Url)}");
            }
        }
    }
}
