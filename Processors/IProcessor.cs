using System;

using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;

namespace Supplier2Presta.Processors
{
    public interface IProcessor
    {
        event EventDelegates.ProcessEventDelegate OnProductProcessed;

        Tuple<int, int, int> Process(Diff newLines);
    }
}
