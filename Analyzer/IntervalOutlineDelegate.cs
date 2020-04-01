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

        public override bool AcceptsFirstResponder()
        {
            return true;
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
            return;
        }
    }

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

            IntervalStackView view = (IntervalStackView)outlineView.MakeView(CellIdentifier, this);
            NSTextField textView = (NSTextField)(view == null ? null : view.Subviews[0]);
            NSButton select = (NSButton)(view == null ? null : view.Subviews[1]);

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
                    Layer = gradientLayer,
                    WantsLayer = true,
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

                select = new NSButton
                {
                    BezelStyle = NSBezelStyle.Rounded,
                    Title = "Текст"
                };
                select.SetButtonType(NSButtonType.MomentaryPushIn);
                select.SetFrameSize(new CGSize(70, 20));

                select.BecomeFirstResponder();
            }

            // Setup view based on the column selected
            textView.StringValue = "Время выполнения: " +
                interval.Info.times.exec_time + "\n" +
                "Коэффициент эффективности: " +
                interval.Info.times.efficiency;
            select.Activated += (object sender, EventArgs e) =>
                Select_Activated(item);
            view.AddView(textView, NSStackViewGravity.Leading);
            view.AddView(select, NSStackViewGravity.Trailing);

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
