using System;
using System.IO;

namespace Analyzer
{
    public class Stat
    {

        public Interval Interval;
        public StatJson Info;

        public string ToJson()
        {
            return UseStatJson.GetJson(Info);
        }

        public Stat(string json, string dir)
        {
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, dir);
        }

        public Stat(string path)
        {
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, Path.GetDirectoryName(path));
        }

        public string GetInfoForStatDir()
        {
            return Info.inter[0].id.pname + " nproc: " + Info.nproc
                + " exec_time: " + Info.inter[0].times.exec_time;
        }
    }
}
