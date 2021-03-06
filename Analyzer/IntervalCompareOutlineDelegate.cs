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
        public double maxTimeLost;
        public double maxTimeGPU;

        public IntervalCompareOutlineDelegate(NSOutlineView outlineView, PlotView plotView) {
            this.plotView = plotView;
            this.outlineView = outlineView;
            maxTimeLost = -1;
            maxTimeGPU = -1;
        }

        public override void SelectionDidChange(NSNotification notification) {
            switch (plotView.Model.Title) {
                case "Потерянное время":
                    plotView.Model = PlotMaker
                        .LostTimeComparePlot(((Interval) outlineView.ItemAtRow(outlineView.SelectedRow)).Row,
                        maxTimeLost);
                    break;
                case "ГПУ":
                    plotView.Model = PlotMaker
                        .GPUComparePlot(((Interval)outlineView.ItemAtRow(outlineView.SelectedRow)).Row,
                        maxTimeGPU);
                    break;
            }
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data

            NSClipView view = (NSClipView)outlineView.MakeView(CellIdentifier, this);
            NSTextField exprView = (NSTextField)(view == null ? null : view.Subviews[0]);
            NSBox line = (NSBox)(view == null ? null : view.Subviews[1]);

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
                    WantsLayer = true,
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

            if (ViewController.CompareList.GetCount() <= 4)
            {
                view.Layer = null;
                
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
            }
            else
            {
                int maxNum, minNum, maxLostNum;
                (maxNum, minNum, maxLostNum) = GetMaxMinStats(intervals);

                NSTextField textView = new NSTextField
                {
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false
                };
                textView.StringValue = "Max " + ViewController.CompareList
                      .At(maxNum).Info.p_heading.Replace('*', 'x') + "\nMin "
                    + ViewController.CompareList
                      .At(minNum).Info.p_heading.Replace('*', 'x');
                textView.SetFrameSize(textView.FittingSize);
                textView.SetFrameOrigin(new CGPoint(line.Frame.Location.X + 10, 0));

                NSTextField textView1 = new NSTextField
                {
                    Selectable = false,
                    Editable = false,
                    DrawsBackground = false,
                    Bordered = false
                };

                textView1.SetFrameOrigin(new CGPoint(textView.Frame.Location.X + textView.Frame.Width + 4, 0));

                textView1.StringValue = " ➢  " + intervals[maxNum].times.exec_time.ToString("F3") + "s\n"
                + " ➢  " + intervals[minNum].times.exec_time.ToString("F3") + "s";
                textView1.SetFrameSize(textView1.FittingSize);

                var gradientLayer = MakeGradLayer(intervals[maxLostNum], false);
                view.WantsLayer = true;
                view.Layer = gradientLayer;
                view.AddSubview(textView);
                view.AddSubview(textView1);
            }

            //---  Устанавливаем значение Expr  ---//
            switch (interval.Info.id.t) {
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

            return view;
        }

        private CAGradientLayer MakeGradLayer(IntervalJson inter, bool isVertical = true)
        {
            var gradientLayer = new CAGradientLayer();
            List<CGColor> colors = new List<CGColor>();
            if (inter.times.comm >= 0.15 * maxTimeLost)
                colors.Add(OxyColors.GreenYellow.ToCGColor());
            if (inter.times.idle >= 0.15 * maxTimeLost)
                colors.Add(OxyColors.LightSkyBlue.ToCGColor());
            if (inter.times.insuf_user >= 0.15 * maxTimeLost)
                colors.Add(OxyColors.Orchid.ToCGColor());
            if (inter.times.insuf_sys >= 0.15 * maxTimeLost)
                colors.Add(OxyColors.Pink.ToCGColor());
            if (isVertical) {
                if (colors.Count == 1)
                    colors.Add(colors[0]);
                gradientLayer.StartPoint = new CGPoint(.0, 1.0);
                gradientLayer.EndPoint = new CGPoint(.0, .0);
            }
            else
            {
                colors.Insert(0, OxyColors.Transparent.ToCGColor());
                gradientLayer.StartPoint = new CGPoint(.0, .0);
                gradientLayer.EndPoint = new CGPoint(1.0, .0);
            }
            gradientLayer.Colors = colors.ToArray();
            return gradientLayer;
        }

        private (int maxNum, int minNum, int maxLostNum)
            GetMaxMinStats(List<IntervalJson> intervals)
        {
            int maxNum = 0, minNum = 0, maxLostNum = 0;
            for(int i = 0; i < intervals.Count; ++i)
            {
                if (intervals[i].times.exec_time > intervals[maxNum].times.exec_time)
                    maxNum = i;
                if (intervals[i].times.exec_time < intervals[minNum].times.exec_time)
                    minNum = i;
                if (intervals[i].times.lost_time > intervals[maxLostNum].times.lost_time)
                    maxLostNum = i;
            }
            return (maxNum, minNum, maxLostNum);
        }

    }
}
