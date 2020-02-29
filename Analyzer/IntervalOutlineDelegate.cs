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
        private ViewController viewController;

        private IntervalOutlineDataSource DataSource;

        public IntervalOutlineDelegate(IntervalOutlineDataSource datasource,
            ViewController viewController, NSOutlineView outlineView)
        {
            this.viewController = viewController;
            DataSource = datasource;
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data

            NSStackView view = (NSStackView)outlineView.MakeView(CellIdentifier, this);
            NSTextView textView = (NSTextView) (view == null ? null : view.Subviews[0]);
            NSButton select = (NSButton)(view == null
                || view.Subviews.Length < 2 ? null : view.Subviews[1]);

            if (view == null)
            {
                view = new NSStackView();
                view.SetFrameSize(new CGSize(tableColumn.Width, 50));
                
                textView = new NSTextView();
                textView.Identifier = CellIdentifier;
                textView.BackgroundColor = NSColor.Clear;
                textView.Selectable = false;
                textView.Editable = false;
                textView.SetFrameSize(new CGSize(tableColumn.Width - 75, 50));
                if (tableColumn.Title == "Details")
                {
                    select = new NSButton
                    {
                        BezelStyle = NSBezelStyle.ThickerSquare,
                        Title = "Select"
                    };
                    select.SetButtonType(NSButtonType.MomentaryPushIn);
                    select.SetFrameSize(new CGSize(70, 20));
                    select.ControlSize = NSControlSize.Regular;
                }
            }

            // Cast item
            var interval = item as Interval;
            

            // Setup view based on the column selected
            switch (tableColumn.Title)
            {
                case "Interval":
                    textView.Value = "Уровень вложенности: "
                        + interval.Info.id.nlev + "\n["
                        + interval.Info.id.nline + ", "
                        + interval.Info.id.nline_end + "]";
                    view.AddView(textView, NSStackViewGravity.Top);
                    break;
                case "Details":
                    textView.Value = "Время выполнения: " +
                        interval.Info.times.exec_time + "\n" +
                        "Коэффициент эффективности: " +
                        interval.Info.times.efficiency;
                    select.Activated += (object sender, EventArgs e) =>
                        Select_Activated(item);
                    view.AddView(textView, NSStackViewGravity.Top);
                    view.AddView(select, NSStackViewGravity.Bottom);
                    break;
            }

            return view;
        }

        private void Select_Activated(object item)
        {
            //Console.Write("Button clicked");
            Interval inter = (Interval)item;
            viewController.Selected(inter.Text);
        }
    }
}
