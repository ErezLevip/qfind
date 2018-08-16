using System.Collections.Generic;

namespace qfind.Interfaces
{
    public interface IDisplayResults
    {
         int ShowResultsAndGetSelection(List<string> results, string search);
    }
}