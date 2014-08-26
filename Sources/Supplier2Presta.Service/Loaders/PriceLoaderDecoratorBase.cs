using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class PriceLoaderDecoratorBase : IPriceLoader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public virtual PriceLoadResult Load(string uri, string encoding)
        {
            if (!File.Exists(uri))
            {
                Log.Fatal("Price file is not exists");
                return new PriceLoadResult(null, PriceLoadResultType.PriceFileIsNotExists);
            }

            Log.Debug("Loading the price. Path: {0}", uri);

            List<string> priceLines = File.ReadLines(uri, Encoding.GetEncoding(encoding)).ToList();
            Log.Debug("{0} lines are loaded", priceLines.Count.ToString(CultureInfo.InvariantCulture));

            return new PriceLoadResult(priceLines, uri, PriceLoadResultType.Ok);
        }
    }
}
