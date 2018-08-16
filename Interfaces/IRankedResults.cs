using System.Collections.Generic;
using System.Linq;
using qfind.Entities;

namespace qfind.Interfaces
{
    public interface IRankedResults
    {
        IOrderedEnumerable<RankedResult> Get(IEnumerable<Index> results, List<Selection> selections, int displayResultsCount);
    }
}