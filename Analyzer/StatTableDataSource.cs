using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace Analyzer
{
    public class StatTableDataSource : NSTableViewDataSource
    {
        public List<StatDir> StatDirs = new List<StatDir>();

        public StatTableDataSource()
        {
        }

        public override nint GetRowCount(NSTableView tableView)
        {
            return StatDirs.Count;
        }

        public override void SortDescriptorsChanged(NSTableView tableView, NSSortDescriptor[] oldDescriptors)
        {
            if (oldDescriptors.Length <= 0)
                return;
            var asc = oldDescriptors[0].Ascending;
            StatDirs.Sort((StatDir x, StatDir y)
                => (asc ? 1 : -1) * DateTime.Compare(x.creationTime, y.creationTime));
            tableView.ReloadData();
        }

    }

}
