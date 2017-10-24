namespace Supplier2Presta.PriceItemBuilders
{
    public interface IColorBuilder
    {
        string GetCode(string colorName);

        string GetMainColor(string colorName);
    }
}
