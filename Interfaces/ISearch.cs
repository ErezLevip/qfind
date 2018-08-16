using System.Collections.Generic;
using System.Threading.Tasks;
using qfind.Entities;

namespace qfind.Interfaces
{
    public interface ISearch
    {
        Task<IEnumerable<RankedResult>> Get(string pattern,FindOptions options);
        Task Select(RankedResult selection);
    }
}