using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public interface IColorBuilder
    {
        string GetCode(string colorName);

        string GetMainColor(string colorName);
    }
}
