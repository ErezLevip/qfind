using System.Collections.Generic;
using qfind.Interfaces;
using System.Linq;
using qfind.Utils;
using System.Threading.Tasks;
using qfind.Entities;

namespace qfind.Implementations
{
    public class ImportIndexer : IIndexer
    {
        private static List<KeyValuePair<string, Index>> _loadedIndexes = new List<KeyValuePair<string, Index>>();
        private readonly IDbAdapter _dbAdapter;
        private readonly DefaultsConfigSection _defaultsConfig;
        public ImportIndexer(IDbAdapter dbAdapter, DefaultsConfigSection defaultsConfig)
        {
            _dbAdapter = dbAdapter;
            _defaultsConfig = defaultsConfig;
        }

        private void LoadAllIndexes(IEnumerable<Index> indexes)
        {
            _loadedIndexes = indexes.Select(i => new KeyValuePair<string, Index>(i.SearchKey, i)).ToList();
        }

        public async Task<IEnumerable<Index>> Get(IEnumerable<string> keys, bool fileNameOnly, bool explicitValue)
        {
            var allIndexes = await _dbAdapter.GetAllIndexes();
            LoadAllIndexes(allIndexes);
            return await SearchUtils.Search(_defaultsConfig.MaxNumberOfTasks, _loadedIndexes, keys, fileNameOnly, explicitValue);
        }
        public void Set(IEnumerable<Index> newIndexes, bool overrideAll)
        {
            _dbAdapter.SetIndexes(newIndexes, overrideAll);
        }
    }
}