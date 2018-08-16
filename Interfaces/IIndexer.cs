using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qfind.Entities;

namespace qfind.Interfaces
{
    public interface IIndexer
    {
        Task<IEnumerable<Index>> Get(IEnumerable<string> keys,bool fileNameOnly,bool explicitValue);
        void Set(IEnumerable<Index> newIndexes,bool overrideAll);
    }
}