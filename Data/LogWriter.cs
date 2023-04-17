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

        string writerpath = "./log.txt";

        public LogWriter()
        {
            _logtext = "";
            streamWriter = new StreamWriter(writerpath);
            streamWriter.AutoFlush = true;

            WriteLogln($"Current Dir:{Environment.CurrentDirectory}");

        }

        ~LogWriter()
        {
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
        }

        public void ClearLog()
        {
            _logtext = string.Empty;
        }

    }
}