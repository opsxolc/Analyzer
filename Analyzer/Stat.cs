using System;
using System.IO;

namespace Analyzer
{
    enum ColOps
    {
        IO, // Input/Output
        RD, // Reduction
        SH, // Shadow renew
        RA // Remote Access
    }

    enum InterTypes
    {
        SEQ = 21,
        PAR,
        USER
    }

    public class Stat
    {
        
        public Interval Interval;
        public StatJson Info;
        public string Dir;

        public string ToJson()
        {
            return UseStatJson.GetJson(Info);
        }

        public Stat(string json, string dir, bool withText = false)
        {
            Dir = dir;
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, dir, withText);
        }

        public Stat(string path, bool withText = false)
        {
            Dir = Path.GetDirectoryName(path);
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, Dir, withText);
        }

        public void ChangeJson(string json, bool withText = false)
        {
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, Dir, withText);
        }

        public string GetInfoForStatDir()
        {
            string res = Info.inter[0].id.pname;
            if (Info.p_heading != null)
                res += "  ∙  " + Info.p_heading.Replace('*', 'x');
            res += "  ∙  " + Info.inter[0].times.exec_time.ToString("F3") + "s";
            return res;
        }

        public uint NumGPU
        {
            get
            {
                uint result = 0;
                for (int i = 0; i < Info.nproc; ++i)
                {
                    result += (uint)Info.inter[0].proc_times[i].num_gpu;
                }
                return result;
            }
        }

    }
}
