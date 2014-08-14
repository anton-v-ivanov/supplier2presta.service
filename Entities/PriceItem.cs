using System.Globalization;

namespace Supplier2Presta.Entities
{
    public class PriceItem
    {
        public string Name { get; set; }

        public string SupplierName { get; set; }

        public string SupplierReference { get; set; }

        public string Reference { get; set; }

        public float WholesalePrice { get; set; }
        
        public float RetailPrice { get; set; }

        public string Category { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public int Balance { get; set; }

        public bool Active { get; set; }

        public string Ean13 { get; set; }

        public string Photo1 { get; set; }

        public string Photo2 { get; set; }

        public string Photo3 { get; set; }

        public string Photo4 { get; set; }

        public string Photo5 { get; set; }

        public string Manufacturer { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public string Battery { get; set; }

        public string Material { get; set; }

        public string Country { get; set; }

        public string Packing { get; set; }

        public string Length { get; set; }

        public string Diameter { get; set; }

        public string Weight { get; set; }

        private string Features
        {
            get
            {
                var features = string.Empty;
                if (!string.IsNullOrWhiteSpace(Size))
                {
                    features += string.Format("{0}|", Size);
                }

                if (!string.IsNullOrWhiteSpace(Color))
                {
                    features += string.Format("{0}|", Color);
                }

                if (!string.IsNullOrWhiteSpace(Material))
                {
                    features += string.Format("{0}|", Material);
                }

                if (!string.IsNullOrWhiteSpace(Country))
                {
                    features += string.Format("{0}|", Country);
                }

                if (!string.IsNullOrWhiteSpace(Packing))
                {
                    features += string.Format("{0}|", Packing);
                }

                if (!string.IsNullOrWhiteSpace(Length))
                {
                    features += string.Format("{0}|", Length);
                }

                if (!string.IsNullOrWhiteSpace(Diameter))
                {
                    features += string.Format("{0}", Diameter);
                }

                return features;
            }
        }

        public string ToString(string format)
        {
            return format.Replace("{{Name}}", this.Name)
                .Replace("{{Reference}}", this.Reference)
                .Replace("{{SupplierName}}", this.SupplierName)
                .Replace("{{SupplierReference}}", this.SupplierReference)
                .Replace("{{WholesalePrice}}", this.WholesalePrice.ToString(CultureInfo.InvariantCulture))
                .Replace("{{RetailPrice}}", this.RetailPrice.ToString(CultureInfo.InvariantCulture))
                .Replace("{{Category}}", this.Category)
                .Replace("{{Manufacturer}}", this.Manufacturer)
                .Replace("{{ShortDescription}}", this.ShortDescription)
                .Replace("{{Description}}", this.Description)
                .Replace("{{Photo}}", this.Photo1)
                .Replace("{{Balance}}", this.Balance.ToString(CultureInfo.InvariantCulture))
                .Replace("{{Features}}", this.Features);
        }
    }
}