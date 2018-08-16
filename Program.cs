using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using qfind.Entities;
using qfind.Implementations;
using qfind.Interfaces;
using qfind.Utils;

namespace qfind
{
    class Program
    {
        const string DB_CONFIG_FILE_NAME = "dbconfig.json";
        const string DEFAULTS_CONFIG_FILE_NAME = "defaultsconfig.json";
        static int Main(string[] args)
        {
            var defaultsConfig = ConfigurationsUtils.LoadConfigFile<DefaultsConfigSection>(DEFAULTS_CONFIG_FILE_NAME);

            var serviceProvider = new ServiceCollection()
                .AddScoped<IIndexer, ImportIndexer>()
                .AddScoped<IDbAdapter, SqliteAdapter>()
                .AddScoped<IRankedResults, RankedResults>()
                .AddScoped<ISearch, Search>()
                .AddScoped<IDisplayResults, TerminalDisplayResults>()
                .AddSingleton(ConfigurationsUtils.LoadConfigFile<DbConfigSection>(DB_CONFIG_FILE_NAME))
                .AddSingleton(defaultsConfig)
                .BuildServiceProvider();

            var app = new CommandLineApplication();
            app.Command("find", (command) =>
            {
                command.Description = "search a file";
                var valuesOption = command.Option("--v", "search for the exact value", CommandOptionType.SingleValue);
                var maxResultsOption = command.Option("--m", "Max result count", CommandOptionType.SingleValue);
                var displayInTermialOption = command.Option("--t", "Dont open the result in file explorer, display in terminal", CommandOptionType.SingleValue);
                var explicitValueOption = command.Option("--e", "Show only results with exact match", CommandOptionType.SingleValue);

                var t = command.Arguments;
                command.OnExecute(async () =>
                       {
                           int resultsCount = defaultsConfig.DefaultResultCount;
                           if (maxResultsOption.HasValue())
                           {
                               if (maxResultsOption.Value() == "*")
                                   resultsCount = -1;
                               else
                                   resultsCount = int.Parse(maxResultsOption.Value());
                           }
                           var cmd = new Command
                           {
                               Type = CommandType.Find,
                               Value = valuesOption.Value()
                           };

                           var findOptions = new FindOptions(explicitValueOption.Value(), resultsCount, displayInTermialOption.Value());

                           await Find(serviceProvider, cmd, findOptions);
                           return 0;

                       });
            });

            app.Command("map", (command) =>
            {
                command.Description = "map a folder";
                var updateOption = command.Option("--u", "update mapping info of a file", CommandOptionType.SingleValue);
                var newPathOption = command.Option("--n", "map a new value to a folder or a file", CommandOptionType.SingleValue);
                var pathOption = command.Option("--v", "map a file or folder and all of its sub folders", CommandOptionType.SingleValue);
                var mapAllOption = command.Option("--a", "Map all the sub folders starting from the root path from the configuration", CommandOptionType.SingleValue);
                var overrideOption = command.Option("--o", "override all mappings", CommandOptionType.SingleValue);
                var clearAllOption = command.Option("--c", "clear all mappings", CommandOptionType.SingleValue);


                command.OnExecute(async () =>
                       {
                           var dbAdapter = serviceProvider.GetService<IDbAdapter>();

                           if (clearAllOption.HasValue())
                           {
                               await dbAdapter.ClearMappings();
                           }
                           else if (!updateOption.HasValue())
                           {
                               string updatePath = pathOption.Value();
                               if (mapAllOption.HasValue() && bool.Parse(mapAllOption.Value()))
                               {
                                   updatePath = serviceProvider.GetService<DefaultsConfigSection>().RootFolder;
                               }

                               bool overrideAll = overrideOption.HasValue() ? bool.Parse(overrideOption.Value()) : false;
                               System.Console.WriteLine($"mapping {updatePath}");

                               var toMap = new List<Index>();
                               if (File.GetAttributes(updatePath).HasFlag(FileAttributes.Directory))
                               {
                                   IndexUtils.Map(toMap, updatePath);
                                   System.Console.WriteLine(string.Join(",", toMap.Select(m => m.SearchKey)));
                                   dbAdapter.SetIndexes(toMap, overrideAll);
                               }
                               else
                               {
                                   toMap.Add(IndexUtils.CreateUpdateIndex(pathOption.Value().ToLower()));
                                   dbAdapter.SetIndexes(toMap, overrideAll);
                               }
                               System.Console.WriteLine($"{toMap.Count} files mapped");
                           }
                           else
                           {
                               await dbAdapter.UpdateIndexes(IndexUtils.CreateUpdateIndex(pathOption.Value().ToLower(), newPathOption.Value().ToLower()));
                           }
                           return 0;
                       });
            });

            return app.Execute(args);
        }

        private static async Task Find(IServiceProvider serviceProvider, Command cmd, FindOptions options)
        {
            var search = serviceProvider.GetService<ISearch>();
            var results = await search.Get(cmd.Value, options);

            if (results.Any())
            {
                var displayResultsHandler = serviceProvider.GetService<IDisplayResults>();
                var optionsToDisplay = results.Select(r => $"{r.Folder}  =>  {r.Name}{r.Extention}").Distinct().ToList();
                var selection = displayResultsHandler.ShowResultsAndGetSelection(optionsToDisplay, cmd.Value);

                System.Console.WriteLine(Environment.NewLine);
                if (selection != -1)
                {
                    var selectedResult = results.ToArray()[selection];
                    await search.Select(selectedResult);

                    if (!options.DisplayResultInTerminal)
                        FilesUtils.OpenFolder(selectedResult.Folder);
                    else
                        FilesUtils.ChangeDir(selectedResult.Folder);
                }
            }
            else
            {
                System.Console.WriteLine($"qfind didnt find any results for {cmd.Value}");
            }
        }
    }
}
