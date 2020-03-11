using System;
using System.Collections.Generic;

namespace Analyzer
{
    public class StatCompareList
    {
        private List<Stat> list;  // список сравнения

        public StatCompareList() => list = new List<Stat>();

        public void Clear() => list.Clear();

        public int GetCount() => list.Count;

        public void Add(Stat stat)
        {
            string res1, res2;
            list.Add(stat);
            for (int i = 0; i < list.Count - 1; ++i)
            {
                LibraryImport.stat_intersect_(list[i].ToJson(),
                    list[i + 1].ToJson(), out res1, out res2);
                list[i] = new Stat(res1, list[i].Dir);
                list[i + 1] = new Stat(res2, list[i + 1].Dir);
            }
        }

        public Stat At(int i)
        {
            if (i >= 0 && i < list.Count)
                return list[i];
            return null;
        }

    }
}
