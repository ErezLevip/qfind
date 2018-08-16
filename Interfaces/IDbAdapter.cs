using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using qfind.Entities;

namespace qfind.Interfaces
{
    public interface IDbAdapter
    {
         Task<List<Index>> GetAllIndexes();
         void SetIndexes(IEnumerable<Index> indexes,bool overrideAll);
         Task UpdateIndexes(IEnumerable<UpdateIndex> updates);
         Task UpdateIndexes(UpdateIndex updates);
         Task<List<Selection>> GetAllPreviousSelections();
         Task SetSelection(Selection selection);
         Task ClearnSelections();
         Task ClearMappings();
    }
}