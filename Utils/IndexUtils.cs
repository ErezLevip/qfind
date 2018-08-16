using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using qfind.Entities;

namespace qfind.Utils
{
    public class IndexUtils
    {
        public static void Map(List<Index> indexes, string currentFolder)
        {
            try
            {
                if (Directory.Exists(currentFolder))
                {
                    var files = Directory.GetFiles(currentFolder);
                    foreach (var f in files)
                    {
                        var file = f.ToLower();
                        indexes.Add(new Index
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Extention = Path.GetExtension(file),
                            Folder = currentFolder,
                            SearchKey = file,
                        });
                    }
                    var directories = Directory.GetDirectories(currentFolder);
                    foreach (var dir in directories)
                    {
                        Map(indexes, dir);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
        }
        public static UpdateIndex CreateUpdateIndex(string oldPath, string newPath = null)
        {
            newPath = newPath ?? oldPath;
            return new UpdateIndex
            {
                Name = Path.GetFileNameWithoutExtension(newPath),
                Extention = Path.GetExtension(newPath),
                Folder = Path.GetDirectoryName(newPath),
                SearchKey = oldPath,
                NewSearchKey = newPath
            };
        }
    }
}