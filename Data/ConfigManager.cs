using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MechKineticsArtSoftware.Data;

namespace MechKineticsArtSoftware
{
    public class ConfigManager
    {
        
        public ConfigData configData { get; set; }

        public ConfigManager()
        {
            LoadDefaultSetting();
        }


        void LoadDefaultSetting(string default_setting_fname = "./default_setting.json")
        {
            using (StreamReader sr = new StreamReader(default_setting_fname))
            {
                configData = JsonSerializer.Deserialize<ConfigData>(sr.ReadToEnd());
            }
        }

        public void SaveSetting(string file_path="./save_setting.json")
        {
            using(StreamWriter sw = new StreamWriter(file_path))
            {
                sw.Write(JsonSerializer.Serialize(configData));
                sw.Flush();
            }
        }
    }
}