using System.Collections.Generic;
using Supplier2Presta.Entities;

namespace Supplier2Presta.Config
{
    public sealed class SupplierSettings
    {
        public string Name { get; set; }

        public SupplierType Supplier { get; set; }

        public PriceType Type { get; set; }

        public string Url { get; set; }

        public string ArchiveDirectory { get; set; }

        public string Encoding { get; set; }

        public string FormatFile { get; set; }

        public int Discount { get; set; }

        public float DefaultMultiplicator { get; set; }

        public IEnumerable<Multiplicator> Multiplicators { get; set; }
    }
}
