using System.Collections.Generic;

namespace Supplier2Presta.Config
{
    public interface IRobotConfiguration
    {
        string ApiUrl { get; }
        string ApiAccessToken { get; }
        IEnumerable<SupplierSettings> Suppliers { get; }
        IEnumerable<ColorMapping> ColorMappings { get; }
        IEnumerable<string> IgnoredProducts { get; }
    }
}