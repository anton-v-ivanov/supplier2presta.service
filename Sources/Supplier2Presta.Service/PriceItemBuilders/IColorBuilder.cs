namespace Supplier2Presta.Service.PriceItemBuilders
{
    public interface IColorBuilder
    {
        string GetCode(string colorName);

        string GetMainColor(string colorName);
    }
}
