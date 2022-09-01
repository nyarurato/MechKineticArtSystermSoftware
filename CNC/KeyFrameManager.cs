using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using CsvHelper;
using CsvHelper.Expressions;
using System.Globalization;
using CsvHelper.Configuration;

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

        public bool SaveAsCsv(string file_name)
        {
            logwriter.WriteLog($"Save keyframe file as CSV... path: {file_name}");
            try
            {
                if (keyframes.Count <= 0)
                {
                    logwriter.WriteLog("No Keyframe Data. Can't Save as CSV");
                    return false;
                }

                using (var streamWriter = new StreamWriter(file_name, false, Encoding.UTF8))
                {
                    using(var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        
                        //Write Fields
                        csvWriter.WriteField(nameof(Keyframe.index));
                        csvWriter.WriteField(nameof(Keyframe.action));
                        csvWriter.WriteField(nameof(Keyframe.move_speed));
                        csvWriter.WriteField(nameof(Keyframe.wait_time));

                        for (int i = 0; i < keyframes[0].unit_motion.Length; i++)
                        {
                            csvWriter.WriteField($"{nameof(Keyframe.unit_motion)}{i}");

                        }
                        csvWriter.NextRecord();

                        //Write Datas
                        foreach(var kf in keyframes)
                        {
                            csvWriter.WriteField(kf.index);
                            csvWriter.WriteField(kf.action);
                            csvWriter.WriteField(kf.move_speed);
                            csvWriter.WriteField(kf.wait_time);

                            foreach(var val in kf.unit_motion)
                            {
                                csvWriter.WriteField(val);

                            }
                            csvWriter.NextRecord();

                        }
                    }
                   
                    
                }
                logwriter.WriteLogln($"Success Saved!");
                return true;
            }
            catch (Exception e)
            {
                logwriter.WriteLogln($"Failed Save: {e.Message}");
                return false;
            }

        }

        public List<Keyframe> LoadAsCsv(string file_name)
        {
            logwriter.WriteLogln($"Load keyframe file from CSV ... path: {file_name}");
            List<Keyframe> kfs = new List<Keyframe>();

            try
            {
                using (var streamReader = new StreamReader(file_name, Encoding.UTF8))
                {
                    using(var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                    {
                        var isHeader = true;
                        while (csvReader.Read())
                        {
                            if (isHeader)
                            {
                                csvReader.ReadHeader();
                                isHeader = false;
                                continue;
                            }

                            var kf = new Keyframe();
                            kf.index = csvReader.GetField<int>(0);
                            kf.action = csvReader.GetField<Keyframe.Action>(1);
                            kf.move_speed = csvReader.GetField<int>(2);
                            kf.wait_time = csvReader.GetField<float>(3);

                            int unit_num = csvReader.HeaderRecord.Length - 4;
                            float[] unit_motion = new float[unit_num];

                            for(int i = 0; i < unit_num; i++)
                            {
                                unit_motion[i] = csvReader.GetField<float>(4+i);
                            }

                            kf.unit_motion = unit_motion;

                            kfs.Add(kf);

                            
                        }
                    }

                }
                
                logwriter.WriteLogln($"Loaded keyframe file");

            }
            catch (Exception e)
            {
                logwriter.WriteLogln($"Failed Load: {e.Message}");

            }

            keyframes = kfs;
            return kfs;

        }
    }

    

   
}