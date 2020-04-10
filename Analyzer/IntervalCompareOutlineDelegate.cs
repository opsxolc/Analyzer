﻿using System;
using AppKit;
using CoreGraphics;
using Foundation;   
using System.Collections;
using System.Collections.Generic;
using CoreAnimation;
using OxyPlot.Xamarin.Mac;
using OxyPlot;

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
            => plotView.Model = PlotMaker
                .LostTimeComparePlot(((Interval)outlineView.ItemAtRow(outlineView.SelectedRow)).Row, maxTime);

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data

            NSClipView view = (NSClipView)outlineView.MakeView(CellIdentifier, this);
            NSTextField exprView = (NSTextField)(view == null ? null : view.Subviews[0]);
            NSBox line = (NSBox)((view == null) ? null : view.Subviews[1]);

            // Cast item
            var interval = item as Interval;
            var intervals = ViewController.CompareList.IntervalsList[interval.Row];

            if (view == null)
            {
                view = new NSClipView
                {
                    Identifier = CellIdentifier,
                    AutoresizesSubviews = true,
                    DrawsBackground = false,
                    AutoresizingMask = NSViewResizingMask.WidthSizable
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

                line = new NSBox
                {
                    BoxType = NSBoxType.NSBoxSeparator
                };

                line.SetFrameSize(new CGSize(2, 23));
                line.SetFrameOrigin(new CGPoint(exprView.Frame.Width + 3, 5));

                view.AddSubview(exprView);
                view.AddSubview(line);
            } else
                for (int i = view.Subviews.Length - 1; i > 1; --i)
                    view.Subviews[i].RemoveFromSuperview();

            //---  Создаем ячейки для интервалов  ---//
            nfloat offset = 0;
            for (var i = intervals.Count - 1; i >= 0; --i)
            {
                var gradientLayer = MakeGradLayer(intervals[i]);

                var interStack = new NSStackView
                {
                    Orientation = NSUserInterfaceLayoutOrientation.Vertical,
                    WantsLayer = true,
                    AutoresizesSubviews = false
                };

                var val = new NSTextField
                {
                    Alignment = NSTextAlignment.Center,
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false,
                    BackgroundColor = NSColor.Clear,
                    WantsLayer = true
                };

                val.StringValue = intervals[i].times.exec_time.ToString("F1")
                    + "\n" + intervals[i].times.efficiency.ToString("F1");
                gradientLayer.Frame = new CGRect(new CGPoint(0, 0), val.FittingSize);
                interStack.Layer.InsertSublayerBelow(gradientLayer, val.Layer);
                interStack.Alignment = NSLayoutAttribute.CenterY;
                interStack.SetFrameSize(val.FittingSize);
                interStack.AddView(val, NSStackViewGravity.Top);

                offset += val.FittingSize.Width;
                interStack.AutoresizingMask = NSViewResizingMask.MinXMargin;
                interStack.SetFrameOrigin(new CGPoint(view.Bounds.Width - offset, 0));
                view.AddSubview(interStack);
            }

            //---  Устанавливаем значение Expr  ---//
            exprView.StringValue = interval.Info.id.expr.ToString();

            return view;
        }

        private CAGradientLayer MakeGradLayer(IntervalJson inter)
        {
            var gradientLayer = new CAGradientLayer();
            List<CGColor> colors = new List<CGColor>();
            if (inter.times.comm >= 0.2 * maxTime)
                colors.Add(OxyColors.GreenYellow.ToCGColor());
            if (inter.times.idle >= 0.2 * maxTime)
                colors.Add(OxyColors.LightSkyBlue.ToCGColor());
            if (inter.times.insuf_user >= 0.2 * maxTime)
                colors.Add(OxyColors.Orchid.ToCGColor());
            if (inter.times.insuf_sys >= 0.2 * maxTime)
                colors.Add(OxyColors.Pink.ToCGColor());
            if (colors.Count == 1)
                colors.Add(colors[0]);
            gradientLayer.Colors = colors.ToArray();
            gradientLayer.StartPoint = new CGPoint(.0, 1.0);
            gradientLayer.EndPoint = new CGPoint(.0, .0);

            return gradientLayer;
        }

    }
}