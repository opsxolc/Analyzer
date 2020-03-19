﻿using System;
using System.IO;

namespace Analyzer
{
    public class Stat
    {
        
        public Interval Interval;
        public StatJson Info;
        public string Dir;

        public string ToJson()
        {
            return UseStatJson.GetJson(Info);
        }

        public Stat(string json, string dir)
        {
            Dir = dir;
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, dir);
        }

        public Stat(string path)
        {
            Dir = Path.GetDirectoryName(path);
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();
            Info = UseStatJson.GetStat(json);
            Interval = new Interval(Info.inter, Dir);
        }

        public string GetInfoForStatDir()
        {
            return Info.inter[0].id.pname + " nproc: " + Info.nproc
                + " exec_time: " + Info.inter[0].times.exec_time;
        }

        public static void CompareStats(Stat stat1, Stat stat2, out Stat res1, out Stat res2)
        {
            string res_str1, res_str2;
            if (LibraryImport.stat_intersect_(stat1.ToJson(), stat2.ToJson(), out res_str1, out res_str2) != 0)
                throw new Exception("Problems with library import function");
            res1 = new Stat(res_str1, stat1.Dir);
            res2 = new Stat(res_str2, stat2.Dir);
        }

        public string GetLabel()
        {
            return "Кол-во проц.: " + Info.nproc + "\n"
                + "Время выполн.: " + Info.inter[0].times.exec_time + "\n"
                + "Коэф. эффект.: " + Info.inter[0].times.efficiency;
        }
    }
}
