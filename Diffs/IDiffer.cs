using System.Collections.Generic;

namespace Supplier2Presta.Diffs
{
    public interface IDiffer
    {
        Diff GetDiff(List<string> newLines, List<string> oldLines);
    }
}