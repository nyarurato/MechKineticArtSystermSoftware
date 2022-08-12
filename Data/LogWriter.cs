using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LogWriter
{
    string _logtext;

    public string text { get { return _logtext; } }

    public LogWriter()
    {
        _logtext = "";
    }

    public void writeLogln(string s)
    {
        writeLog(s + Environment.NewLine);
    }

    public void writeLog(string s)
    {
        _logtext += s;
    }

}