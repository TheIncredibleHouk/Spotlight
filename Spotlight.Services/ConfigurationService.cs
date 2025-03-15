using Newtonsoft.Json;
using Spotlight.Abstractions;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private Configuration _configuration;

        public ConfigurationService()
        {
            string configPath = GetConfigPath();
            if (File.Exists(configPath))
            {
                _configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configPath));
            }
            else
            {
                _configuration = new Configuration();
            }
        }

        public void UpdateConfiguration(Configuration configuration)
        {
             _configuration = configuration;
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(_configuration, Newtonsoft.Json.Formatting.Indented));
        }
        
        public Configuration GetConfiguration()
        {
            return _configuration;
        }


        private string GetConfigPath()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Spotlight";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory + "/config.json";
        }
    }
}
