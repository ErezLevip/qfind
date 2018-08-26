using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using qfind.Entities;
using System.IO;

namespace qfind.Utils
{
    public static class SearchUtils
    {
        public static async Task<IEnumerable<Index>> Search(int maxNumberOfTasks, List<Index> indexes, IEnumerable<string> keys, bool fileNameOnly, bool explicitValue)
        {
            var tasks = new List<Task<IEnumerable<Index>>>();
            var taskCount = indexes.Count > maxNumberOfTasks ? maxNumberOfTasks : 1;
            int avgAmount = indexes.Count / taskCount;
            bool IsRound = indexes.Count % taskCount > 0;

            for (var i = 0; i < taskCount; i++)
            {
                var items = new List<Index>();
                if (indexes.Count > maxNumberOfTasks)
                {
                    var IsLast = i == maxNumberOfTasks - 1;
                    var startIdx = i * avgAmount;
                    var endIdx = (startIdx + avgAmount) > indexes.Count ? indexes.Count : startIdx + avgAmount + ((IsLast && IsRound) ? 1 : 0);

                    for (var j = startIdx; j <= endIdx - 1; j++)
                    {
                        items.Add(indexes[j]);
                    }
                }
                else
                {
                    items = indexes;
                }

                tasks.Add(Task.Run(() =>
                {
                    return items.Where(item =>
                    {
                        var searchKey = fileNameOnly ? Path.GetFileName(item.SearchKey) : item.SearchKey;
                        if (explicitValue)
                        {
                            return keys.First() == searchKey;
                        }
                        return keys.All(k => searchKey.Contains(k));
                    });
                }));

            }

            await Task.WhenAll(tasks.ToArray());
            return tasks.SelectMany(t => t.Result);
        }
    }
}