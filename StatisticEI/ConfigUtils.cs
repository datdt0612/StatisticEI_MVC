using Newtonsoft.Json;
using StatisticEI.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StatisticEI
{
    public static class ConfigUtils
    {
        public static string GetQLDVConnectionString()
        {
            string configJson = GetConfigJson();
            var config = JsonConvert.DeserializeObject<ConfigModel>(configJson);
            return config!.QldvConn;
        }

        public static List<string> GeAllConnectionString()
        {
            string configJson = GetConfigJson();
            var config = JsonConvert.DeserializeObject<ConfigModel>(configJson);
            return config!.DataConns.ToList();
        }

        private static string GetConfigJson()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "StatisticEI.config.json";

            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            StreamReader streamReader = new StreamReader(stream);
            using StreamReader reader = streamReader;
            return reader.ReadToEnd();
        }
    }
}
