using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace MechKineticsArtSoftware
{
    
    class KeyFrameManager
    {
        public List<Keyframe> keyframes { get; set; }
        XmlSerializer xmlSerializer;
        LogWriter logwriter;

        public KeyFrameManager(LogWriter logger)
        {
            logwriter = logger;
            keyframes = new List<Keyframe>();
            xmlSerializer = new XmlSerializer(typeof(List<Keyframe>));
        }

        public bool Save(string file_name)
        {
            logwriter.WriteLog($"Save keyframe file... path: {file_name}");
            try { 
                using(var streamWriter = new StreamWriter(file_name, false, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(streamWriter, keyframes);
                    streamWriter.Flush();
                }
                logwriter.WriteLogln($"Success Saved!");
                return true;
            }catch(Exception e)
            {
                logwriter.WriteLogln($"Failed Save: {e.Message}");
                return false;
            }
        }

        public List<Keyframe> Load(string file_name)
        {
            logwriter.WriteLogln($"Load keyframe file... path: {file_name}");

            var xmlSettings = new System.Xml.XmlReaderSettings
            {
                CheckCharacters = false,
            };

            try
            {
                using (var streamReader = new StreamReader(file_name, Encoding.UTF8))
                using (var xmlReader = System.Xml.XmlReader.Create(streamReader, xmlSettings))
                {
                    keyframes = (List<Keyframe>)xmlSerializer.Deserialize(xmlReader);
                }
                logwriter.WriteLogln($"Loaded keyframe file");

            }catch(Exception e)
            {
                logwriter.WriteLogln($"Failed Load: {e.Message}");

            }
            return keyframes;
        }
    }

   
}