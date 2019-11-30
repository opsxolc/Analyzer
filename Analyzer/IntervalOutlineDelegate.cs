using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace Analyzer
{
    public class IntervalOutlineDelegate : NSOutlineViewDelegate
    {
        private const string CellIdentifier = "IntervalCell";

        private IntervalOutlineDataSource DataSource;

        public IntervalOutlineDelegate(IntervalOutlineDataSource datasource)
        {
            this.DataSource = datasource;
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            NSTextView view = (NSTextView)outlineView.MakeView(CellIdentifier, this);
            if (view == null)
            {
                view = new NSTextView();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                //view.Bordered = false;
                view.SetFrameSize(new CGSize(50, 50));
                view.Selectable = false;
                view.Editable = false;
            }

            // Cast item
            var interval = item as Interval;

            // Setup view based on the column selected
            switch (tableColumn.Title)
            {
                case "Interval":
                    view.Value = "Уровень вложенности: "
                        + interval.Info.id.nlev + "\n["
                        + interval.Info.id.nline + ", "
                        + interval.Info.id.nline_end + "]";
                    break;
                case "Details":
                    view.Value = "Время выполнения: "
                        + interval.Info.times.exec_time
                        + "\nКоэффициент эффективности: "
                        + interval.Info.times.efficiency;
                    break;
            }

            return view;
        }
    }
}
