using System;
using System.Collections.Generic;

namespace Analyzer
{
    public class StatCompareList
    {
        public readonly List<Stat> List;  // список сравнения
        public readonly List<List<IntervalJson>> IntervalsList;

        public StatCompareList()
        {
            List = new List<Stat>();
            IntervalsList = new List<List<IntervalJson>>();
        }

        public void Clear()
        {
            List.Clear();
            IntervalsList.Clear();
        }

        public int GetCount() => List.Count;

        public void Add(Stat stat)
        {
            string res1, res2;
            string json = stat.ToJson(); 
            for (int i = 0; i < List.Count; ++i)
            {
                // TODO: Починить тупую ошибку с test_time
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

        public void BuildIntervalsList()
        {
            IntervalsList.Clear();
            if (List.Count == 0)
                return;
            for (int i = 0; i < List[0].Info.inter.Count; ++i) {
                List<IntervalJson> intervals = new List<IntervalJson>();
                for (int j = 0; j < List.Count; ++j) { 
                    intervals.Add(List[j].Info.inter[i]);
                }
                IntervalsList.Add(intervals);
            }
        }

        public void PrintIntervalList()
        {
            for (int i = 0; i < IntervalsList.Count; ++i)
            {
                Console.WriteLine("Interval " + i);
                foreach (var inter in IntervalsList[i])
                {
                    Console.Write("  " + inter.times.exec_time);
                }
                Console.WriteLine();
            }
        }

        public void Sort(string par, int intervalNum = 0)
        {
            switch (par)
            {
                case "Кол-во процессоров":
                    List.Sort((Stat st1, Stat st2) => st1.Info.nproc - st2.Info.nproc);
                    break;
                case "Потерянное время":
                    List.Sort((Stat st1, Stat st2) =>
                         (int)(100 * (st1.Info.inter[intervalNum].times.lost_time
                            - st2.Info.inter[intervalNum].times.lost_time)));
                    break;
                case "Время выполнения":
                    List.Sort((Stat st1, Stat st2) =>
                        (int)(100 * (st1.Info.inter[intervalNum].times.exec_time
                            - st2.Info.inter[intervalNum].times.exec_time)));
                    break;
                case "Коэф. эффективности":
                    List.Sort((Stat st1, Stat st2) =>
                        (int)(100 * (st1.Info.inter[intervalNum].times.efficiency
                            - st2.Info.inter[intervalNum].times.efficiency)));
                    break;
            }
            BuildIntervalsList();
        }

    }
}
