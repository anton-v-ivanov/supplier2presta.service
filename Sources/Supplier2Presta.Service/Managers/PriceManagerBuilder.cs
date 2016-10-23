using System.Configuration;
using System.IO;
using System.Reflection;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service.Managers
{
    class PriceManagerBuilder
    {
        public static IPriceManager Build(SupplierElement settings, string apiUrl, string apiAccessToken, IColorBuilder colorCodeBuilder)
        {
            if(settings.Supplier != SupplierType.Happiness)
                throw new ConfigurationErrorsException("There is no price manager for supplier type " + settings.Supplier);

            var archiveDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), settings.ArchiveDirectory);

            IRetailPriceBuilder retailPriceBuilder = new RetailPriceBuilder(settings.Multiplicators);

            switch (Path.GetExtension(settings.Url))
            {
                case ".xml":
                    return new HappinesXmlPriceManager(settings.Url, settings.Discount, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken, colorCodeBuilder);
                
                case ".csv":
                    return new HappinesCsvPriceManager(settings.Url, settings.Discount, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken, settings.PriceFormatFile, settings.PriceEncoding);

                default:
                    throw new ConfigurationErrorsException("There is no price manager for file with extension " + Path.GetExtension(settings.Url));
            }
        }
    }
}
