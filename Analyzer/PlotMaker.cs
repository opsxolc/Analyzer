using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Mac;

namespace Analyzer
{

    public class PlotMaker 
    {

        private static (List<ColumnSeries> data, List<CategoryAxis> xaxis) InitDataAndXaxis()
        {
            List<CategoryAxis> xaxis = new List<CategoryAxis>();
            List<ColumnSeries> data = new List<ColumnSeries>();

            int curOffset = 2, offset = 11;

            for (int i = 0; i < 4; ++i)
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

                data.Add(new ColumnSeries
                {
                    LabelPlacement = LabelPlacement.Middle,
                    LabelFormatString = "{0:0.###}",
                    IsStacked = true
                });
            }

            xaxis[0].MajorGridlineStyle = LineStyle.Solid;
            xaxis[0].MinorGridlineStyle = LineStyle.Dot;

            data[0].FillColor = OxyColors.Pink;
            data[0].Title = "Недостаточный параллелизм (sys)";

            data[1].FillColor = OxyColors.Orchid;
            data[1].Title = "Недостаточный параллелизм (user)";
            //data[0].MouseDown += (object sender, OxyMouseDownEventArgs e)
            //    => Console.WriteLine("data[0] MouseDown "
            //    + e.ClickCount + " " + e.Position);

            data[2].FillColor = OxyColors.LightSkyBlue;
            data[2].Title = "Простои";

            data[3].FillColor = OxyColors.GreenYellow;
            data[3].Title = "Коммуникации";

            return (data, xaxis);
        }

        public static void SortPlot(PlotView view, string par, int intervalNum = 0)
        {
            view.Model.InvalidatePlot(true);
            view.Model.Series.Clear();
            view.Model.InvalidatePlot(true);
            var yaxis = view.Model.GetAxis("Time");
            view.Model.Axes.Clear();

            (var data, var xaxis) = InitDataAndXaxis();

            ViewController.CompareList.Sort(par, intervalNum);
            var compareList = ViewController.CompareList.List;

            List<IntervalJson> intervals = ViewController.CompareList.IntervalsList[intervalNum];
            //for (int i = 0; i < compareList.Count; ++i)
            //    intervals.Add(compareList[i].Interval.GetIntervalAt(intervalNum).Info);

            for (int i = 0; i < compareList.Count; ++i)
            {
                xaxis[0].Labels.Add("Кол-во процессоров: "
                    + compareList[i].Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: "
                    + compareList[i].Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: "
                    + compareList[i].Info.inter[0].times.efficiency.ToString("F3"));
                xaxis[3].Labels.Add("Файл: "
                    + compareList[i].Info.inter[0].id.pname);
                data[0].Items.Add(new ColumnItem(intervals[i].times.insuf_sys));
                data[1].Items.Add(new ColumnItem(intervals[i].times.insuf_user));
                data[2].Items.Add(new ColumnItem(intervals[i].times.idle));
                data[3].Items.Add(new ColumnItem(intervals[i].times.comm));
            }

            view.Model.Axes.Add(yaxis);
            view.Model.InvalidatePlot(true);

            for (int i = 0; i < 4; ++i) {
                view.Model.Axes.Add(xaxis[i]);
                view.Model.InvalidatePlot(true);
                view.Model.Series.Add(data[i]);
                view.Model.InvalidatePlot(true);
            }
            view.Model.InvalidatePlot(true);

        }

        public void DetailPlot(StatCompareList compareList)
        {
            // TODO: Сделать что-то ?
        }

        public static PlotModel LostTimeComparePlot(int intervalNum = 0, double maxTime = -1)
        {
            //Console.WriteLine("Making plot for intervalNum: " + intervalNum);
            var model = new PlotModel
            {
                Title = "Потерянное время"
            };

            //---  Init axis  ---//
            (var data, var xaxis) = InitDataAndXaxis();

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
                xaxis[0].Labels.Add("Кол-во процессоров: "
                    + ViewController.CompareList.At(i).Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: "
                    + ViewController.CompareList.At(i).Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: "
                    + ViewController.CompareList.At(i).Info.inter[0].times.efficiency.ToString("F3"));
                xaxis[3].Labels.Add("Файл: "
                    + ViewController.CompareList.At(i).Info.inter[0].id.pname);
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

    }
}
