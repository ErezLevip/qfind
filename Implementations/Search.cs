using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qfind.Entities;
using qfind.Interfaces;

namespace qfind.Implementations
{
    public class Search : ISearch
    {
        private readonly IIndexer _indexer;
        private readonly IRankedResults _rankedResults;
        private readonly IDbAdapter _dbAdapter;
        public Search(IIndexer indexer, IRankedResults rankedResults, IDbAdapter dbAdapter)
        {
            _indexer = indexer;
            _rankedResults = rankedResults;
            _dbAdapter = dbAdapter;
        }

        public async Task<IEnumerable<RankedResult>> Get(string pattern, FindOptions options)
        {
            if (!string.IsNullOrEmpty(pattern))
            {
                var keys = pattern.Split(' ').Select(k => k.ToLower());
                var previousSelections = await _dbAdapter.GetAllPreviousSelections();
                var results = (await _indexer.Get(keys, options.FileNameOnly,options.ExplicitValue)).ToList();
                return _rankedResults.Get(results, previousSelections, options.MaxResults);
            }
            return new List<RankedResult>().AsEnumerable();
        }

        public async Task Select(RankedResult selection)
        {
            await _dbAdapter.SetSelection(new Selection(selection.Count + 1, selection.SearchKey));
        }
    }
}