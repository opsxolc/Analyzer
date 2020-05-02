using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace Analyzer
{
    public class StatTableDelegate : NSTableViewDelegate
    {
        public StatTableDelegate()
        {
        }

        public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            var dataSource = (StatTableDataSource)tableView.DataSource;
            var item = dataSource.StatDirs[(int)row];
            NSTextField view = (NSTextField)tableView.MakeView(item.hash + tableColumn.Title, this);

            if (view == null)
            {
                view = new NSTextField();
                view.DrawsBackground = false;
                view.Identifier = item.hash + tableColumn.Title;
                view.Bordered = false;
                view.Selectable = false;
                view.Editable = false;
                if (tableColumn.Title == "Статистика выполнения")
                    view.StringValue = item.info;
                else
                    view.StringValue = item.creationTime.ToString();
            }
            return view;
        }
    }
}
