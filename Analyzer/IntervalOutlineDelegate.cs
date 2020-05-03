using System;
using AppKit;
using CoreGraphics;
using Foundation;   
using System.Collections;
using System.Collections.Generic;
using CoreAnimation;
using OxyPlot;
using OxyPlot.Xamarin.Mac;

namespace Analyzer
{
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

            NSClipView view = (NSClipView)outlineView.MakeView(CellIdentifier, this);
            NSTextField exprView = (NSTextField)(view == null ? null : view.Subviews[0]);
            NSTextField textView;
            NSTextField textView1 = (NSTextField)(view == null ? null : view.Subviews[3]);


            // Cast item
            var interval = item as Interval;

            if (view == null)
            {
                

                view = new NSClipView
                {
                    Identifier = CellIdentifier,
                    AutoresizesSubviews = true,
                    BackgroundColor = NSColor.Clear,
                    AutoresizingMask = NSViewResizingMask.WidthSizable,
                    WantsLayer = true
                };

                exprView = new NSTextField
                {
                    Alignment = NSTextAlignment.Center,
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false,
                    LineBreakMode = NSLineBreakMode.Clipping
                };

                exprView.RotateByAngle(-90);

                exprView.SetFrameOrigin(new CGPoint(0, 2));
                exprView.SetFrameSize(new CGSize(13, 28));

                NSBox line = new NSBox
                {
                    BoxType = NSBoxType.NSBoxSeparator
                };

                line.SetFrameSize(new CGSize(2, 23));
                line.SetFrameOrigin(new CGPoint(exprView.Frame.Width + 3, 5));

                textView = new NSTextField
                {
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false
                };
                textView.StringValue = "Время вып.\nКоэф.эффект.";
                textView.SetFrameSize(textView.FittingSize);
                textView.SetFrameOrigin(new CGPoint(line.Frame.Location.X + 10, 0));

                textView1 = new NSTextField
                {
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false
                };

                textView1.SetFrameOrigin(new CGPoint(textView.Frame.Location.X + textView.Frame.Width + 3, 0));

                view.AddSubview(exprView);
                view.AddSubview(line);
                view.AddSubview(textView);
                view.AddSubview(textView1);
            }

            CAGradientLayer gradientLayer = new CAGradientLayer();
            List<CGColor> colors = new List<CGColor>();
            colors.Add(OxyColors.Transparent.ToCGColor());
            if (interval.Info.times.comm >= 0.2 * viewController.plotStatMaxTime)
            {
                colors.Insert(0, OxyColors.Transparent.ToCGColor());
                colors.Add(OxyColors.GreenYellow.ToCGColor());
            }
            if (interval.Info.times.idle >= 0.2 * viewController.plotStatMaxTime)
            {
                colors.Insert(0, OxyColors.Transparent.ToCGColor());
                colors.Add(OxyColors.LightSkyBlue.ToCGColor());
            }
            if (interval.Info.times.insuf_user >= 0.2 * viewController.plotStatMaxTime)
            {
                colors.Insert(0, OxyColors.Transparent.ToCGColor());
                colors.Add(OxyColors.Orchid.ToCGColor());
            }
            if (interval.Info.times.insuf_sys >= 0.2 * viewController.plotStatMaxTime)
            {
                colors.Insert(0, OxyColors.Transparent.ToCGColor());
                colors.Add(OxyColors.Pink.ToCGColor());
            }
            if (colors.Count == 1)
                colors.Add(colors[0]);
            gradientLayer.Colors = colors.ToArray();
            gradientLayer.StartPoint = new CGPoint(.0, .0);
            gradientLayer.EndPoint = new CGPoint(1.0, .0);
            view.Layer = gradientLayer;

            // Setup view based on the column selected
            switch (interval.Info.id.t)
            {
                case (int)InterTypes.USER:
                    exprView.StringValue = interval.Info.id.expr.ToString();
                    break;
                case (int)InterTypes.SEQ:
                    exprView.StringValue = "Посл";
                    exprView.Font = NSFont.FromFontName("Helvetica Neue", 10);
                    break;
                case (int)InterTypes.PAR:
                    exprView.StringValue = "Пар";
                    exprView.Font = NSFont.FromFontName("Helvetica Neue", 10);
                    break;
            }

            textView1.StringValue = "➢   " + interval.Info.times.exec_time.ToString("F3") + "s\n"
                + "➢   " + interval.Info.times.efficiency.ToString("F3");
            textView1.SetFrameSize(textView1.FittingSize);

            return view;
        }

        public override void SelectionDidChange(NSNotification notification)
        {
            viewController.InterView_SelectionDidChange(this, new EventArgs());
        }

    }
}
