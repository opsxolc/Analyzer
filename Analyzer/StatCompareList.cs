using System;
using System.Collections.Generic;

namespace Analyzer
{
    public class StatCompareList
    {
        public readonly List<Stat> List;  // список сравнения

        public StatCompareList() => List = new List<Stat>();

        public void Clear() => List.Clear();

        public int GetCount() => List.Count;

        public void Add(Stat stat)
        {
            string res1, res2;
            //List.Add(stat);
            string json = stat.ToJson(); 
            for (int i = 0; i < List.Count; ++i)
            {
                //Console.WriteLine("Going to LibraryImport");
                //Console.WriteLine("STAT1: \n" + List[i].Interval + "\n");
                //Console.WriteLine("STAT2: \n" + new Stat(json, "").Interval + "\n");
                LibraryImport.stat_intersect_(List[i].ToJson(), json, out res1, out res2);
                //Console.WriteLine("LibraryImport OK");
                //Console.WriteLine("RES1: \n" + new Stat(res1, "").Interval + "\n");
                //Console.WriteLine("RES2: \n" + new Stat(res2, "").Interval + "\n");
                List[i].ChangeJson(res1);
                json = res2;
            }
            stat.ChangeJson(json);
            List.Add(stat);
        }

        public Stat At(int i)
        {
            if (i >= 0 && i < List.Count)
                return List[i];
            return null;
        }

    }
}
