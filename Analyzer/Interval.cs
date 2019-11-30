using System;
using System.Collections.Generic;
using Foundation;

namespace Analyzer
{
    public class Interval : NSObject
    {
        public List<Interval> Intervals = new List<Interval>();
        public IntervalJson Info;

        public bool HasChildIntervals
        {
            get { return (Intervals.Count > 0); }
        }

        public Interval()
        {
        }

        public override string ToString()
        {
            string res = "Interval: " + Info.id.nlev
                + " " + Info.id.t + " " + Info.id.nline;
            foreach (Interval inter in Intervals)
                res += "\n" + inter;
            return res;
        }

        public Interval(List<IntervalJson> intervals)
        {
            if (intervals == null)
                return;
            int i = 1;
            Info = intervals[0];
            while (i < intervals.Count)
            {
                if (intervals[i].id.nlev == Info.id.nlev + 1)
                {
                    int j = i + 1;
                    while (j < intervals.Count && intervals[j].id.nlev > intervals[i].id.nlev)
                        ++j;
                    Intervals.Add(new Interval(intervals.GetRange(i, j - i)));
                    i = j - 1;
                }
                ++i;
            }
        }
    }
}
