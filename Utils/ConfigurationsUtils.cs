using System.IO;
using Newtonsoft.Json;
using qfind.Entities;

namespace qfind.Utils
{
    public class ConfigurationsUtils
    {
        public static T LoadConfigFile<T>(string name)
        {
            var configFile = File.ReadAllText(name);
            return JsonConvert.DeserializeObject<T>(configFile);
        }
    }
}