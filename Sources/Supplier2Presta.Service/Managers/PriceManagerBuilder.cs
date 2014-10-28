using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.PriceBuilders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Managers
{
    class PriceManagerBuilder
    {
        public static IPriceManager Build(SupplierElement settings, string apiUrl, string apiAccessToken)
        {
            if(settings.Supplier != SupplierType.Happiness)
                throw new ConfigurationErrorsException("There is no price manager for supplier type " + settings.Supplier);

            var archiveDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), settings.ArchiveDirectory);

            IRetailPriceBuilder retailPriceBuilder = new RetailPriceBuilder(settings.Multiplicators);

            switch (Path.GetExtension(settings.Url))
            {
                case ".xml":
                    return new HappinesXmlPriceManager(settings, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken);
                
                case ".csv":
                    return new HappinesCsvPriceManager(settings, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken);

                default:
                    throw new ConfigurationErrorsException("There is no price manager for file with extension " + Path.GetExtension(settings.Url));
            }
        }
    }
}
