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
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace MechKineticsArtSoftware
{
    public class ConfigManager
    {
        
        public ConfigData configData { get; set; }
        public string userDataPath { get; private set; }
        public string ncDataPath { get; private set; }
        public string logDataPath { get; private set; }
        public string user_default_configPath { get; private set; }
        const string user_defalt_config_filename = "UserDefaultConfig.json";


        public ConfigManager()
        {            
            DirectoryInitialize();
            
            //ユーザーデフォルト設定の確認
            user_default_configPath = Path.Combine(userDataPath, user_defalt_config_filename);
            if (!File.Exists(user_default_configPath))//フォルダなしの場合
            {
                LoadDefaultSetting();
            }
            else
            {
                LoadDefaultSetting(user_default_configPath);
            }
        }

        void DirectoryInitialize()
        {
            //設定等保存フォルダを作成
            var userpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MKAS");

            if (!File.Exists(userpath))//フォルダなしの場合
            {
                Directory.CreateDirectory(userpath);
            }
            userDataPath = userpath;

            //NC保存フォルダ作成
            var ncpath = Path.Combine(userDataPath, "nc");
            if (!File.Exists(ncpath))//フォルダなしの場合
            {
                Directory.CreateDirectory(ncpath);
            }
            ncDataPath = ncpath;

            //ログ保存フォルダ作成
            var logpath = Path.Combine(userDataPath, "log");
            if (!File.Exists(logpath))//フォルダなしの場合
            {
                Directory.CreateDirectory(logpath);
            }
            logDataPath = logpath;

            
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
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
                {
                    // null 値プロパティ除外
                    IgnoreNullValues = true,
                    // 文字コード
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    //整形出力
                    WriteIndented = true,
                };
                sw.Write(JsonSerializer.Serialize(cf,jsonSerializerOptions));
                sw.Flush();
            }
        }

    }
}