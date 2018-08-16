using System.Collections.Generic;
using System.Linq;
using qfind.Entities;
using qfind.Interfaces;

namespace qfind.Implementations
{
    public class RankedResults : IRankedResults
    {
        public void RankedResult()
        {
        }
        public IOrderedEnumerable<RankedResult> Get(IEnumerable<Index> results, List<Selection> selections, int displayResultsCount)
        {
            var rankedResults = new List<RankedResult>();
            var relevantPreviousSelections = results.Where(r => selections.Any(s => s.SearchKey.Equals(r.SearchKey)));

            rankedResults.AddRange(relevantPreviousSelections.Select(rps => new RankedResult
            {
                Folder = rps.Folder,
                Name = rps.Name,
                DateCreated = rps.DateCreated,
                Extention = rps.Extention,
                SearchKey = rps.SearchKey,
                Count = selections.First(s => s.SearchKey == rps.SearchKey).Count
            }));

            var rankedResultsCountAfterSelections = rankedResults.Count();

            if (rankedResultsCountAfterSelections >= displayResultsCount)
            {
                //rankedResults.Take(displayResultsCount);
            }
            else
            {
                var matchesToFind = displayResultsCount - rankedResultsCountAfterSelections;
                rankedResults.AddRange(results.Where(r => !relevantPreviousSelections.Any(rps => rps.SearchKey.Equals(r.SearchKey)))
                .Take(matchesToFind).Select(rps => new RankedResult
                {
                    Folder = rps.Folder,
                    Name = rps.Name,
                    DateCreated = rps.DateCreated,
                    Extention = rps.Extention,
                    SearchKey = rps.SearchKey,
                    Count = 0
                }));

                if (displayResultsCount > rankedResults.Count)
                {
                    var leftToFill = displayResultsCount - rankedResults.Count;
                    rankedResults.AddRange(results.Where(r => !rankedResults.Any(rr => rr.SearchKey == r.SearchKey)).Take(leftToFill).Select(rps => new RankedResult
                    {
                        Folder = rps.Folder,
                        Name = rps.Name,
                        DateCreated = rps.DateCreated,
                        Extention = rps.Extention,
                        SearchKey = rps.SearchKey,
                        Count = 0
                    }));
                }
            }

            return rankedResults.Take(displayResultsCount).OrderByDescending(r => r.Count);
        }
    }
}