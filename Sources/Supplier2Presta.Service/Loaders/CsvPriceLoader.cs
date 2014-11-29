using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service.Loaders
{
    public class CsvPriceLoader : IPriceLoader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly string _encoding;
        private readonly PriceFormat _priceFormat;

        public CsvPriceLoader(string encoding, PriceFormat priceFormat)
        {
            _encoding = encoding;
            _priceFormat = priceFormat;
        }

        public PriceLoadResult Load<T>(string uri)
        {
            if (!File.Exists(uri))
            {
                Log.Fatal("Price file is not exists");
                return new PriceLoadResult(null, false);
            }

            Log.Debug("Loading the price. Path: {0}", uri);

            List<string> priceLines = File.ReadLines(uri, Encoding.GetEncoding(_encoding)).ToList();
            Log.Debug("{0} lines are loaded", priceLines.Count.ToString(CultureInfo.InvariantCulture));

            var priceItemBuilder = new CsvHappinnessPriceItemBuilder(_priceFormat);
            var localLines = priceLines.Skip(1).ToList(); // пропуск строки с заголовками
            var newItems = localLines.Select(priceItemBuilder.Build);

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
