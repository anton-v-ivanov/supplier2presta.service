namespace Supplier2Presta.Loaders
{
    public interface IPriceLoader
    {
        PriceLoadResult Load<T>(string uri);
    }
}
