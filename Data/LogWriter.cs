using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Encodings;
using CsvHelper;

namespace MechKineticsArtSoftware
{
    public class LogWriter
    {
        string _logtext;

        public string text { get { return _logtext; } }

        StreamWriter streamWriter;

        string writerpath = "./";
        string log_filename = "log.txt";

        public LogWriter(ConfigManager configManager)
        {
            _logtext = "";

            if (configManager.logDataPath != "")
                writerpath = configManager.logDataPath;

            writerpath = Path.Combine(configManager.logDataPath, log_filename);

            streamWriter = new StreamWriter(writerpath);
            streamWriter.AutoFlush = true;

            WriteLogln($"Current Dir:{Environment.CurrentDirectory}");

        }

        ~LogWriter()
        {
            streamWriter.Flush();
            streamWriter.Close();
        }

        public void WriteLogln(string s)
        {
            WriteLog(s + Environment.NewLine);
        }

        public void WriteLog(string s)
        {
            _logtext += DateTime.Now.ToString("yy/MM/dd HH:mm:ss") + ": " + s;
            Console.Write(s);
            streamWriter.Write(s);
            _ = streamWriter.FlushAsync();
        }

        public void ClearLog()
        {
            _logtext = string.Empty;
        }

    }
}