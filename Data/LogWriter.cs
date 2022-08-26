using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Encodings;

namespace MechKineticsArtSoftware
{
    public class LogWriter
    {
        string _logtext;

        public string text { get { return _logtext; } }

        StreamWriter streamWriter;

        public LogWriter()
        {
            _logtext = "";
            streamWriter = new StreamWriter("./log.txt");
            streamWriter.AutoFlush = true;
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
            _logtext += s;
            Console.Write(s);
            streamWriter.Write(s);
        }

        public void ClearLog()
        {
            _logtext = string.Empty;
        }

    }
}