using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MechKineticsArtSoftware.Data;
using System.Threading.Tasks;

namespace MechKineticsArtSoftware
{
    public class ConfigManager
    {
        
        public ConfigData configData { get; set; }

        public ConfigManager()
        {
            LoadDefaultSetting();
        }

        public ConfigData GetClonedConfigData()
        {
            return JsonSerializer.Deserialize<ConfigData>(JsonSerializer.Serialize(configData));
        }
        public ConfigData LoadSetting(string file_path = "./default_setting.json")
        {
            using (StreamReader sr = new StreamReader(file_path))
            {
                return JsonSerializer.Deserialize<ConfigData>(sr.ReadToEnd());
            }
        }

        void LoadDefaultSetting(string default_setting_fname = "./default_setting.json")
        {

            configData = LoadSetting(default_setting_fname);
            
        }

        public void SaveSetting(string file_path="./save_setting.json")
        {
            using(StreamWriter sw = new StreamWriter(file_path))
            {
                sw.Write(JsonSerializer.Serialize(configData));
                sw.Flush();
            }
        }

        public void SaveSetting(ConfigData cf,string file_path = "./save_setting.json")
        {
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                sw.Write(JsonSerializer.Serialize(cf));
                sw.Flush();
            }
        }

    }
}