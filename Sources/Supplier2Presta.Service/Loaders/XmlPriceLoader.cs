using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.PricefileItemBuilders;
using Supplier2Presta.Service.PriceItemBuilders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Supplier2Presta.Service.Loaders
{
    public class XmlPriceLoader : IPriceLoader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public PriceLoadResult Load<T>(string uri)
        {
            if (!File.Exists(uri))
            {
                Log.Fatal("Price file is not exists");
                return new PriceLoadResult(null, false);
            }

            Log.Debug("Loading the price. Path: {0}", uri);

            var serializer = new XmlSerializer(typeof(T));
            IPriceXmlItem xmlItems;
            using (var stream = new FileStream(uri, FileMode.Open))
            {
                xmlItems = (IPriceXmlItem)serializer.Deserialize(stream);
            }

            Log.Debug("{0} lines are loaded", xmlItems.Items.Count.ToString(CultureInfo.InvariantCulture));
            
            var priceItemBuilder = new XmlHappinnessPriceItemBuilder();
            var newItems = xmlItems.Items.Select(priceItemBuilder.Build);

            var prods = new Dictionary<string, PriceItem>();

            foreach (var item in newItems)
            {
                if (!prods.ContainsKey(item.Reference))
                {
                    prods.Add(item.Reference, item);
                }
            }

            return new PriceLoadResult(prods, uri, true);
        }
    }
}
