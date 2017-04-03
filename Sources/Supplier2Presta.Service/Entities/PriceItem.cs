using System.Collections.Generic;

namespace Supplier2Presta.Service.Entities
{
    public class PriceItem
    {
        public string Name { get; set; }

        public string SupplierName { get; set; }

        public string SupplierReference { get; set; }

        public string Reference { get; set; }

        public float WholesalePrice { get; set; }
        
        public float RetailPrice { get; set; }

    public List<CategoryInfo> Categories { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }

        public string Ean13 { get; set; }

        public string Manufacturer { get; set; }

        public string Battery { get; set; }

        public string Material { get; set; }

        public string Country { get; set; }

        public string Packing { get; set; }

        public string Length { get; set; }

        public string Diameter { get; set; }

        public string Weight { get; set; }

        public bool OnSale { get; set; }

        public int DiscountValue { get; set; }

        public string PhotoSmall { get; set; }

        public List<string> Photos { get; set; }

        public List<Assort> Assort { get; set; }

        public PriceItem()
        {
            Assort = new List<Assort>();
            Photos = new List<string>();
        }
    }
}