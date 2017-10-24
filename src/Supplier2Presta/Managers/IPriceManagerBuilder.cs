using Supplier2Presta.Config;

namespace Supplier2Presta.Managers
{
    public interface IPriceManagerBuilder
    {
        IPriceManager Build(SupplierSettings supplierSettings);
    }
}
