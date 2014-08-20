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
                Log.Fatal(string.Format("Файл прайса не существует"));
                return new PriceLoadResult(null, PriceLoadResultType.PriceFileIsNotExists);
            }

            Log.Debug("Загрузка прайса. Путь {0}", uri);

            List<string> priceLines = File.ReadLines(uri, Encoding.GetEncoding(encoding)).ToList();
            Log.Debug("Загружено {0} строк", priceLines.Count.ToString(CultureInfo.InvariantCulture));

            return new PriceLoadResult(priceLines, uri, PriceLoadResultType.Ok);
        }
    }
}
