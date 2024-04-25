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
            
            //���[�U�[�f�t�H���g�ݒ�̊m�F
            user_default_configPath = Path.Combine(userDataPath, user_defalt_config_filename);
            if (!File.Exists(user_default_configPath))//�t�H���_�Ȃ��̏ꍇ
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
            //�ݒ蓙�ۑ��t�H���_���쐬
            var userpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MKAS");

            if (!File.Exists(userpath))//�t�H���_�Ȃ��̏ꍇ
            {
                Directory.CreateDirectory(userpath);
            }
            userDataPath = userpath;

            //NC�ۑ��t�H���_�쐬
            var ncpath = Path.Combine(userDataPath, "nc");
            if (!File.Exists(ncpath))//�t�H���_�Ȃ��̏ꍇ
            {
                Directory.CreateDirectory(ncpath);
            }
            ncDataPath = ncpath;

            //���O�ۑ��t�H���_�쐬
            var logpath = Path.Combine(userDataPath, "log");
            if (!File.Exists(logpath))//�t�H���_�Ȃ��̏ꍇ
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
                    // null �l�v���p�e�B���O
                    IgnoreNullValues = true,
                    // �����R�[�h
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    //���`�o��
                    WriteIndented = true,
                };
                sw.Write(JsonSerializer.Serialize(cf,jsonSerializerOptions));
                sw.Flush();
            }
        }

    }
}