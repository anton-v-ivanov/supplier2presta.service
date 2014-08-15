using System;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.Processors
{
    public interface IProcessor
    {
        event EventDelegates.ProcessEventDelegate OnProductProcessed;

        Tuple<int, int, int> Process(Diff newLines);
    }
}
