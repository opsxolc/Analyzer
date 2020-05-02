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
        public string Text = "";
        public bool HasText = false;
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

        public bool HasChildLoopInterval
        {
            get
            {
                foreach (var inter in Intervals)
                {
                    if (inter.Info.id.t != (int)InterTypes.USER)
                        return true;
                }
                bool result = false;
                int i = 0;
                while (i < Intervals.Count && !result)
                {
                    result |= Intervals[i].HasChildLoopInterval;
                    ++i;
                }
                return result;
            }
        }

        public Interval(List<IntervalJson> intervals, string dir, bool withText, int row = 0)
        {
            Row = row;
            if (intervals == null)
                return;
            int i = 1;
            Info = intervals[0];
            //TODO: Добавить чтение текста по надобности
            if (withText)
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
                if (intervals[i].id.nlev == Info.id.nlev + 1)
                {
                    int j = i + 1;
                    while (j < intervals.Count && intervals[j].id.nlev > intervals[i].id.nlev)
                        ++j;
                    Intervals.Add(new Interval(intervals.GetRange(i, j - i), dir, withText, row + 1));
                    row += j - i - 1;
                    i = j - 1;
                }
                ++row;
                ++i;
            }
        }
    }
}
