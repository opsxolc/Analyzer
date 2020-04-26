using System;
using AppKit;
using CoreGraphics;
using Foundation;   
using System.Collections;
using System.Collections.Generic;
using CoreAnimation;

namespace Analyzer
{
    public class IntervalStackView : NSStackView
    {
        private Interval item;
        private DateTime lastClick;
        private NSWindowController controller;
        private static NSStoryboard storyboard = NSStoryboard.FromName("Main", null);

        public IntervalStackView(Interval item)
        {
            this.item = item;
            lastClick = DateTime.Now;
            //TODO: Use normal window
            //controller = storyboard.InstantiateControllerWithIdentifier("IntervalCompare") as NSWindowController;
            //controller.Window.Title = "Some title " + item.Info.times.exec_time;
        }

        public override void MouseDown(NSEvent theEvent)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(lastClick).TotalMilliseconds < 500.0)
            {
                // Actions on DoubleClick
                //controller.ShowWindow(this);
            }
            lastClick = now;
            base.MouseDown(theEvent);
            return;
        }
    }

    public class IntervalOutlineDelegate : NSOutlineViewDelegate
    {
        private const string CellIdentifier = "IntervalCell";
        private ViewController viewController;

        private IntervalOutlineDataSource DataSource;

        public IntervalOutlineDelegate(IntervalOutlineDataSource datasource,
            ViewController viewController)
        {
            this.viewController = viewController;
            DataSource = datasource;
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data

            IntervalStackView view = (IntervalStackView)outlineView.MakeView(CellIdentifier, this);
            NSTextField textView = (NSTextField)(view == null ? null : view.Subviews[0]);

            // Cast item
            var interval = item as Interval;

            if (view == null)
            {
                CAGradientLayer gradientLayer = new CAGradientLayer();
                CGColor[] colors = { CGColor.CreateSrgb(1, 1, 1, 0),
                    CGColor.CreateSrgb(0, 1, (nfloat)0.1, (nfloat)0.7) };
                gradientLayer.Colors = colors;
                gradientLayer.StartPoint = new CGPoint(.0, .0);
                gradientLayer.EndPoint = new CGPoint(1.0, .0);

                view = new IntervalStackView(interval)
                {
                    //Layer = gradientLayer,
                    //WantsLayer = true,
                    AutoresizesSubviews = false
                };

                view.SetFrameSize(new CGSize(tableColumn.Width - 20, 50));

                textView = new NSTextField
                {
                    Identifier = CellIdentifier,
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false
                };

                textView.SetFrameSize(new CGSize(tableColumn.Width - 130, 50));
            }

            // Setup view based on the column selected
            textView.StringValue = "type - " + (InterTypes) interval.Info.id.t + "  expr - " + interval.Info.id.expr;
            view.AddView(textView, NSStackViewGravity.Leading);

            return view;
        }

        public override void SelectionDidChange(NSNotification notification)
        {
            viewController.InterView_SelectionDidChange(this, new EventArgs());
        }

    }
}
