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
            List.Add(stat);
            for (int i = 0; i < List.Count - 1; ++i)
            {
                LibraryImport.stat_intersect_(List[i].ToJson(),
                    List[i + 1].ToJson(), out res1, out res2);
                List[i] = new Stat(res1, List[i].Dir);
                List[i + 1] = new Stat(res2, List[i + 1].Dir);
            }
        }

        public Stat At(int i)
        {
            if (i >= 0 && i < List.Count)
                return List[i];
            return null;
        }

    }
}
