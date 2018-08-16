using System;
using qfind;
using qfind.Entities;

namespace qfind.Utils
{
    public static class CommandUtils
    {
        public static Command ProcessCommand(string[] args)
        {
            if (args.Length < 2)
                throw new InvalidOperationException("Args are missing");

            var cmdTypeArg = args[0];
            var valueArg = args[1];

            CommandType cmdTyp;

            switch (cmdTypeArg)
            {
                case "-mf":
                    cmdTyp = CommandType.MapFolder;
                    break;
                case "-ma":
                    cmdTyp = CommandType.MapAll;
                    break;
                case "-t":
                    cmdTyp = CommandType.OpenValueInConsole;
                    break;
                case "-v":
                default:
                    cmdTyp = CommandType.Find;
                    break;
            }
            return new Command
            {
                Type = cmdTyp,
                Value = valueArg
            };
        }
    }
}