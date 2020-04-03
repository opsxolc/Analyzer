using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;

namespace Analyzer
{
    public class Interval : NSObject
    {
        public List<Interval> Intervals = new List<Interval>();
        public IntervalJson Info;
        public String Text = "";
        public bool HasText;
        public int Row;

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
                + " " + Info.id.expr + " " + Info.id.nline;
            foreach (Interval inter in Intervals)
                res += "\n" + inter;
            return res;
        }

        public Interval GetIntervalAt(int index)
        {
            if (Row == index)
                return this;
            int i = 0;
            while (i + 1 < Intervals.Count && index >= Intervals[i + 1].Row)
                ++i;
            if (i < Intervals.Count)
                return Intervals[i].GetIntervalAt(index);
            return null;
        }

        public Interval(List<IntervalJson> intervals, string dir, int row = 0)
        {
            Row = row;
            if (intervals == null)
                return;
            int i = 1;
            Info = intervals[0];
            try { 
                var Lines = File.ReadLines(dir + '/' + Info.id.pname);
                for (int j = Info.id.nline - 1; j < Info.id.nline_end; ++j)
                    Text += Lines.ElementAt(j) + '\n';
                HasText = true;
            } catch (Exception e)
            {
                Text = e.Message;
            }
            while (i < intervals.Count)
            {
                Interval tmp = null;
                if (intervals[i].id.nlev == Info.id.nlev + 1)
                {
                    int j = i + 1;
                    while (j < intervals.Count && intervals[j].id.nlev > intervals[i].id.nlev)
                        ++j;
                    Intervals.Add(tmp = new Interval(intervals.GetRange(i, j - i), dir, ++row));
                    i = j - 1;
                }
                if (tmp != null) {
                    if (tmp.Intervals.Count > 0)
                        row = tmp.Intervals[tmp.Intervals.Count - 1].Row;
                    else
                        row = tmp.Row;
                }

                ++i;
            }
        }
    }
}
