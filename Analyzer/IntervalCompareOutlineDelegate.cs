using System;
using AppKit;
using CoreGraphics;
using Foundation;   
using System.Collections;
using System.Collections.Generic;
using CoreAnimation;
using OxyPlot.Xamarin.Mac;

namespace Analyzer
{

    public class IntervalCompareOutlineDelegate : NSOutlineViewDelegate
    {
        private const string CellIdentifier = "IntervalCell";
        private PlotView plotView;
        private NSOutlineView outlineView;
        private double maxTime;

        public IntervalCompareOutlineDelegate(NSOutlineView outlineView, PlotView plotView){
            this.plotView = plotView;
            this.outlineView = outlineView;
            maxTime = -1;
        }

        public void SetMaxTime(double maxTime) => this.maxTime = maxTime;

        public override void SelectionDidChange(NSNotification notification)
            => plotView.Model = PlotMaker.LostTimeComparePlot((int)outlineView.SelectedRow, maxTime);

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data

            NSStackView view = (NSStackView)outlineView.MakeView(CellIdentifier, this);
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

                view = new NSStackView()
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

                textView.SetFrameSize(new CGSize(tableColumn.Width - 20, 50));

            }

            textView.StringValue = "Время выполнения: " +
                string.Format("{0:f2}", interval.Info.times.exec_time) + "\n" +
                "Коэфф. эффективности: " +
                string.Format("{0:f2}", interval.Info.times.efficiency);
            view.AddView(textView, NSStackViewGravity.Leading);

            return view;
        }

    }
}
