using System.Collections.Generic;

namespace Supplier2Presta.Config
{
    public class RobotConfiguration: IRobotConfiguration
    {
        public string ApiUrl { get; }
        public string ApiAccessToken { get; }
        public IEnumerable<SupplierSettings> Suppliers { get; }
        public IEnumerable<ColorMapping> ColorMappings { get; }
        public IEnumerable<string> IgnoredProducts { get; }

        public RobotConfiguration(string apiUrl, string apiAccessToken, 
            IEnumerable<SupplierSettings> suppliers, IEnumerable<ColorMapping> colorMappings, IEnumerable<string> ignoredProducts)
        {
            ApiUrl = apiUrl;
            ApiAccessToken = apiAccessToken;
            Suppliers = suppliers;
            ColorMappings = colorMappings;
            IgnoredProducts = ignoredProducts;
        }
    }
}
