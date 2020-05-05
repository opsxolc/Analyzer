using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using CoreGraphics;
using OxyPlot.Xamarin.Mac;
using AppKit;

namespace Analyzer
{

    public interface PlotPopoverInterface
    {
        CGSize Init(int statNum, int interNum, NSWindow window = null);
    }

    public class PlotMaker 
    {
        public static CGPoint clickPoint = new CGPoint(0,0);

        private static DateTime procSelectionLastClick = DateTime.Now;

        private static (List<ColumnSeries> data, List<CategoryAxis> xaxis) InitDataAndXaxis(int interNum, bool popover = true)
        {
            List<CategoryAxis> xaxis = new List<CategoryAxis>();
            List<ColumnSeries> data = new List<ColumnSeries>();

            int curOffset = 2, offset = 11;

            for (int i = 0; i < 5; ++i)
            {
                xaxis.Add(new CategoryAxis
                {
                    FontSize = 10,
                    Position = AxisPosition.Bottom,
                    IsZoomEnabled = false,
                    IsPanEnabled = false,
                    AxisTickToLabelDistance = curOffset
                });
                curOffset += offset;

                if (i < 4) { 
                    data.Add(new ColumnSeries
                    {
                        LabelPlacement = LabelPlacement.Middle,
                        LabelFormatString = "{0:0.###}",
                        IsStacked = true
                    });

                    if (popover)
                        data[i].MouseDown += (object sender, OxyMouseDownEventArgs e)
                            => Model_MouseDown(sender, e, interNum);
                }
            }

            xaxis[0].MajorGridlineStyle = LineStyle.Solid;
            xaxis[0].MinorGridlineStyle = LineStyle.Dot;

            data[0].FillColor = OxyColors.Pink;
            data[0].Title = "Недостаточный параллелизм (sys)";

            data[1].FillColor = OxyColors.Orchid;
            data[1].Title = "Недостаточный параллелизм (user)";

            data[2].FillColor = OxyColors.LightSkyBlue;
            data[2].Title = "Простои";

            data[3].FillColor = OxyColors.GreenYellow;
            data[3].Title = "Коммуникации";

            return (data, xaxis);
        }

        public void DetailPlot(StatCompareList compareList)
        {
            // TODO: Сделать что-то ?
        }

        public static PlotModel LostTimeComparePlot(int intervalNum = 0, double maxTime = -1)
        {
            var model = new PlotModel
            {
                Title = "Потерянное время"
            };

            //---  Init axis  ---//
            (var data, var xaxis) = InitDataAndXaxis(intervalNum);

            var yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;
            if (maxTime > 0)
                yaxis.Maximum = maxTime;
            yaxis.Key = "Time";
             
            List<IntervalJson> intervals = ViewController.CompareList.IntervalsList[intervalNum];

            for (int i = 0; i < ViewController.CompareList.GetCount(); ++i)
            {
                xaxis[0].Labels.Add(ViewController.CompareList.At(i).Info
                    .p_heading.Replace('*', 'x'));
                xaxis[1].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .times.exec_time.ToString("F3") + "s");
                xaxis[2].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .times.efficiency.ToString("F3"));
                xaxis[3].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .id.pname);
                data[0].Items.Add(new ColumnItem(intervals[i].times.insuf_sys));
                data[1].Items.Add(new ColumnItem(intervals[i].times.insuf_user));
                data[2].Items.Add(new ColumnItem(intervals[i].times.idle));
                data[3].Items.Add(new ColumnItem(intervals[i].times.comm));
            }

            for (int i = 0; i < 4; ++i)
            {
                model.Axes.Add(xaxis[i]);
                model.Series.Add(data[i]);
            }
            model.Axes.Add(yaxis);

            return model;
        }

        public static PlotModel ProcLostTimePlot(Stat stat, ViewController viewController,
            int intervalNum = 0, double maxTime = -1)
        {
            var model = new PlotModel
            {
                Title = "Потерянное время"
            };

            //---  Init axis  ---//
            (var data, var _) = InitDataAndXaxis(intervalNum, false);

            var yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;
            if (maxTime > 0)
                yaxis.Maximum = maxTime;
            yaxis.Key = "Time";

            var xaxis = new CategoryAxis
            {
                Title = "Процессоры",
                AbsoluteMinimum = -0.5
            };
            for (int i = 0; i < stat.Info.nproc; ++i) {
                xaxis.Labels.Add((i + 1).ToString());
                data[0].Items.Add(new ColumnItem(stat.Info.inter[intervalNum].proc_times[i].insuf_sys));
                data[1].Items.Add(new ColumnItem(stat.Info.inter[intervalNum].proc_times[i].insuf_user));
                data[2].Items.Add(new ColumnItem(stat.Info.inter[intervalNum].proc_times[i].idle));
                data[3].Items.Add(new ColumnItem(stat.Info.inter[intervalNum].proc_times[i].comm));
                //Console.WriteLine(intervalNum + " - " + i + "  |  "
                //    + stat.Info.inter[intervalNum].proc_times[i].insuf_sys + " - "
                //    + stat.Info.inter[intervalNum].proc_times[i].insuf_user + " - "
                //    + stat.Info.inter[intervalNum].proc_times[i].idle + " - "
                //    + stat.Info.inter[intervalNum].proc_times[i].comm);
            }
            for (int i = 0; i < 4; ++i)
            {
                data[i].MouseDown += (object sender, OxyMouseDownEventArgs e)
                    => ProcLostTimePlot_MouseDown(sender, e, viewController, intervalNum);
                model.Series.Add(data[i]);
            }
            model.Axes.Add(xaxis);
            model.Axes.Add(yaxis);
            model.MouseDown += (object sender, OxyMouseDownEventArgs e)
                => ProcLostTimePlotModel_MouseDown(sender, e, model, viewController);

            if (ViewController.SelectedProc >= 0)
                foreach (var ser in model.Series)
                {
                    var color = (ser as ColumnSeries).FillColor;
                    model.InvalidatePlot(true);
                    for (int j = 0; j < (ser as ColumnSeries).Items.Count; ++j)
                        (ser as ColumnSeries).Items[j].Color =
                            color.ChangeSaturation((j == ViewController.SelectedProc) ? 1 : 0.5);
                }

            return model;
        }

        private static void ProcLostTimePlotModel_MouseDown(object sender, OxyMouseDownEventArgs e,
            PlotModel model, ViewController viewController)
        {
            if (e.HitTestResult is null && e.ClickCount >= 2)
            {
                foreach (var ser in model.Series)
                {
                    var color = (ser as ColumnSeries).FillColor;
                    model.InvalidatePlot(true);
                    for (int j = 0; j < (ser as ColumnSeries).Items.Count; ++j)
                        (ser as ColumnSeries).Items[j].Color = color.ChangeSaturation(1);
                }
                viewController.DeselectProcessors();
            }
        }

        private static void ProcLostTimePlot_MouseDown(object sender, OxyMouseEventArgs e,
            ViewController viewController, int intervalNum)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(procSelectionLastClick).TotalMilliseconds < 500.0)
            {
                var s = sender as ColumnSeries;
                var nearest = s.GetNearestPoint(e.Position, false);
                var i = (int)nearest.DataPoint.X;
                var model = s.PlotModel;
                foreach (var ser in model.Series)
                {
                    var color = (ser as ColumnSeries).FillColor;
                    model.InvalidatePlot(true);
                    for (int j = 0; j < (ser as ColumnSeries).Items.Count; ++j)
                        (ser as ColumnSeries).Items[j].Color =
                            color.ChangeSaturation((j == i) ? 1 : 0.5);
                }
                viewController.SelectProcessor(i, intervalNum);
            }
            procSelectionLastClick = now;
        }

        private static void Model_MouseDown(object sender, OxyMouseDownEventArgs e, int interNum)
        {
            var columns = sender as ColumnSeries;
            var model = columns.PlotModel;
            var nearest = columns.GetNearestPoint(e.Position, false);
            string name;

            if (columns.FillColor == OxyColors.GreenYellow) { 
                name = "CommPopover";
            }
            else if (columns.FillColor == OxyColors.LightSkyBlue) { 
                name = "IdlePopover";
            }
            else {
                name = "InsufPopover";
            }

            var windowController = ViewController.storyboard
                .InstantiateControllerWithIdentifier(name) as NSWindowController;
            var viewController = windowController.ContentViewController as PlotPopoverInterface;

            CGSize size = viewController.Init((int)nearest.DataPoint.X, interNum, (model.PlotView as NSView).Window);

            var popover = new NSPopover
            {
                ContentSize = size,
                Behavior = NSPopoverBehavior.Transient,
                Animates = true,
                ContentViewController = viewController as NSViewController
            };
            
            popover.Show(new CGRect(clickPoint, new CGSize(1, 1)),
                   model.PlotView as PlotView, NSRectEdge.MinXEdge);
        }

        public static PlotModel GPUComparePlot(int intervalNum = 0, double maxTime = -1)
        {
            var model = new PlotModel
            {
                Title = "ГПУ"
            };

            //---  Init axi  ---//
            (_, var xaxis) = InitDataAndXaxis(intervalNum);

            //---  Init data  ---//
            LineSeries prodGPU = new LineSeries
            {
                Title = "Продуктивное время на ГПУ",
                Color = OxyColors.LawnGreen,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 5,
                MarkerFill = OxyColors.Snow,
                MarkerResolution = 5,
                MarkerStroke = OxyColors.LawnGreen,
                MarkerStrokeThickness = 3,
                LineStyle = LineStyle.Dash
            };
            LineSeries lostGPU = new LineSeries
            {
                Title = "Потерянное время на ГПУ",
                Color = OxyColors.Salmon,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 5,
                MarkerFill = OxyColors.Snow,
                MarkerStroke = OxyColors.Salmon,
                MarkerStrokeThickness = 3,
                LineStyle = LineStyle.Dash
            };
            LineSeries execTime = new LineSeries
            {
                Title = "Время выполнения",
                Color = OxyColors.RoyalBlue,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 5,
                MarkerFill = OxyColors.Snow,
                MarkerStroke = OxyColors.RoyalBlue,
                MarkerStrokeThickness = 3,
                LineStyle = LineStyle.Dash
            };

            var yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;
            if (maxTime > 0)
                yaxis.Maximum = maxTime;
            yaxis.Key = "Time";

            List<IntervalJson> intervals = ViewController.CompareList.IntervalsList[intervalNum];

            for (int i = 0; i < ViewController.CompareList.GetCount(); ++i)
            {
                xaxis[0].Labels.Add("GPU ➢ "
                    + ViewController.CompareList.At(i).NumGPU);
                xaxis[1].Labels.Add(ViewController.CompareList.At(i).Info
                    .p_heading.Replace('*', 'x'));
                xaxis[2].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .times.exec_time.ToString("F3") + "s");
                xaxis[3].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .times.efficiency.ToString("F3"));
                xaxis[4].Labels.Add(ViewController.CompareList.At(i).Info.inter[intervalNum]
                    .id.pname);
                prodGPU.Points.Add(new DataPoint(i, intervals[i].times.gpu_time_prod));
                lostGPU.Points.Add(new DataPoint(i, intervals[i].times.gpu_time_lost));
                execTime.Points.Add(new DataPoint(i, intervals[i].times.exec_time));
            }

            for (int i = 0; i < 5; ++i)
            {
                model.Axes.Add(xaxis[i]);
            }
            model.Series.Add(prodGPU);
            model.Series.Add(lostGPU);
            model.Series.Add(execTime);
            model.Axes.Add(yaxis);

            return model;
        }

    }
}
