using System;
using System.Collections.Generic;
using AppKit;

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

    }

}
