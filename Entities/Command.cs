using System;
using qfind.Utils;

namespace qfind.Entities
{
    public class Command
    {
        public CommandType Type { get; set; }
        public string Value { get; set; }
    }
    public enum CommandType
    {
        Find = 0,
        MapFolder,
        MapAll,
        OpenValueInConsole,
        Update
    }
    public enum FileChangeType
    {
        Create = 0,
        Delete ,
        Rename,
        Move,
    }
}