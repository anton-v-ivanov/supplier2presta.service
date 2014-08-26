using System;

namespace Supplier2Presta.Service.Entities
{
    [Serializable]
    public class PriceFormat
    {
        public int Name { get; set; }

        public int SupplierReference { get; set; }

        public int Reference { get; set; }

        public int WholesalePrice { get; set; }

        public int RetailPrice { get; set; }

        public int Manufacturer { get; set; }

        public int Balance { get; set; }

        public int Active { get; set; }

        public int Ean13 { get; set; }

        public int Description { get; set; }

        public int Size { get; set; }

        public int Color { get; set; }

        public int Battery { get; set; }

        public int Material { get; set; }

        public int Weight { get; set; }
        
        public int Country { get; set; }

        public int Packing { get; set; }

        public int Category1 { get; set; }

        public int Category2 { get; set; }

        public int Category3 { get; set; }

        public int Thumbnail { get; set; }

        public int Photo1 { get; set; }

        public int Photo2 { get; set; }

        public int Photo3 { get; set; }

        public int Photo4 { get; set; }

        public int Photo5 { get; set; }
    }
}
