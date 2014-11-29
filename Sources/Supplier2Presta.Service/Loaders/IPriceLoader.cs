namespace Supplier2Presta.Service.Loaders
{
    public interface IPriceLoader
    {
        PriceLoadResult Load<T>(string uri);
    }
}
