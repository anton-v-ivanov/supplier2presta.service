using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.Processors
{
    public class PriceFileProcessor : IProcessor
    {
        private readonly string filePath;

        private readonly int oneFileCount;

        private readonly IDiffer differ;

        public PriceFileProcessor(string filePath, int oneFileCount, IDiffer differ)
        {
            this.filePath = filePath;
            this.oneFileCount = oneFileCount;
            this.differ = differ;
        }

        public event EventDelegates.ProcessEventDelegate OnProductProcessed;

        public Tuple<int, int, int> Process(Diff diff)
        {
            int sameCount = GenerateProducts(diff.SameItems.Values.ToList(), filePath, oneFileCount, GeneratedPriceType.SameItems);
            int newCount = GenerateProducts(diff.NewItems.Values.ToList(), filePath, oneFileCount, GeneratedPriceType.NewItems);
            int deletedCount = GenerateProducts(diff.DeletedItems.Values.ToList(), filePath, oneFileCount, GeneratedPriceType.DeletedItems);
            
            return Tuple.Create(sameCount, newCount, deletedCount);
        }

        private int GenerateProducts(IEnumerable<PriceItem> items, string filePath, int oneFileCount, GeneratedPriceType generatedPriceType)
        {
            string header;
            string format;

            switch (generatedPriceType)
            {
                case GeneratedPriceType.NewItems:
                    header = "имя;артикул;поставщик;артикул_поставщика;цена_оптовая;цена_розничная;категория;производитель;короткое_описание;длинное_описание;фото;остатки;характеристики;активен;" + Environment.NewLine;
                    format = "{{Name}};{{Reference}};{{SupplierName}};{{SupplierReference}};{{WholesalePrice}};{{RetailPrice}};{{Category}};{{Manufacturer}};{{ShortDescription}};{{Description}};{{Photo}};{{Balance}};{{Features}};1";
                    break;
                case GeneratedPriceType.DeletedItems:
                    header = "имя;артикул;активен;" + Environment.NewLine;
                    format = "{{Name}};{{Reference}};0;";
                    break;
                case GeneratedPriceType.SameItems:
                    header = "имя;артикул;цена_оптовая;цена_розничная;остатки;активен;" + Environment.NewLine;
                    format = "{{Name}};{{Reference}};{{WholesalePrice}};{{RetailPrice}};{{Balance}};1";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("generatedPriceType");
            }

            var sb = new StringBuilder();

            int count = 0;
            int totalCount = 0;
            int fileNameCount = 1;

            foreach (var item in items)
            {
                sb.AppendLine(item.ToString(format));

                totalCount++;
                count++;
                if (count >= oneFileCount)
                {
                    sb.Insert(0, header);
                    File.WriteAllText(Path.GetDirectoryName(filePath) + string.Format(Helper.GetFileName(generatedPriceType), fileNameCount), sb.ToString(), Encoding.UTF8);
                    sb.Clear();
                    count = 0;
                    fileNameCount++;
                }

                if (OnProductProcessed != null)
                {
                    OnProductProcessed(item.Name, generatedPriceType);
                }
            }

            sb.Insert(0, header);
            File.WriteAllText(Path.GetDirectoryName(filePath) + string.Format(Helper.GetFileName(generatedPriceType), fileNameCount), sb.ToString(), Encoding.UTF8);

            return totalCount;
        }
    }
}