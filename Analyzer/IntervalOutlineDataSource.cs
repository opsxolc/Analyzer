using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace Analyzer
{
    public class IntervalOutlineDataSource : NSOutlineViewDataSource
    {
        public List<Interval> Intervals = new List<Interval>();

        public IntervalOutlineDataSource()
        {
        }

        public override nint GetChildrenCount(NSOutlineView outlineView, NSObject item)
        {
            if (item == null)
            {
                return Intervals.Count;
            }
            else
            {
                return ((Interval)item).Intervals.Count;
            }
        }

        public override NSObject GetChild(NSOutlineView outlineView, nint childIndex, NSObject item)
        {
            if (item == null)
            {
                return Intervals[(int)childIndex];
            }
            else
            {
                return ((Interval)item).Intervals[(int)childIndex];
            }

        }


        public override bool ItemExpandable(NSOutlineView outlineView, NSObject item)
        {
            if (item == null)
            {
                return Intervals[0].HasChildIntervals;
            }
            else
            {
                return ((Interval)item).HasChildIntervals;
            }
        }
    }
}