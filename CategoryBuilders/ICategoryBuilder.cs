using System.Collections.Generic;

namespace Supplier2Presta.CategoryBuilders
{
    public interface ICategoryBuilder
    {
        int Build(IEnumerable<string> lines);
    }
}