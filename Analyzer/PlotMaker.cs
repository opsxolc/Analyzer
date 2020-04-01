using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Mac;

namespace Analyzer
{

    public class PlotMaker  // TODO: Разобраться с конструктором и compareList'ом (нужен рефакторинг)
    {

        private PlotView plotView;
        private StatCompareList compareList;
        public PlotModel baseModel;
        public PlotModel detailModel;
        public PlotModel intervalModel;
        private LinearAxis yaxis;

        public PlotMaker(PlotView plotView)
        {
            this.plotView = plotView;
            plotView.Model = new PlotModel();
        }

        public void ResetPlot()
        {
            plotView.Model = null;
        }

        public void BasePlot(StatCompareList compareList)
        {
            this.compareList = compareList;

            baseModel = new PlotModel();
            baseModel.Title = "Потерянное время";

            (var data, var xaxis) = InitDataAndXaxis();
            
            yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;
            yaxis.Key = "Time";

            foreach (var item in compareList.List)
            {
                xaxis[0].Labels.Add("Кол-во процессоров: " + item.Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: " + item.Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: " + item.Info.inter[0].times.efficiency.ToString("F3"));
                data[0].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf_sys));
                data[1].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf_user));
                data[2].Items.Add(new ColumnItem(item.Info.inter[0].times.idle));
                data[3].Items.Add(new ColumnItem(item.Info.inter[0].times.comm));
            }

            for (int i = 0; i < 3; ++i)
            {
                baseModel.Axes.Add(xaxis[i]);
                baseModel.Series.Add(data[i]);
            }
            baseModel.Series.Add(data[3]);
            baseModel.Axes.Add(yaxis);

            plotView.Model = baseModel;
        }

        private (List<ColumnSeries> data, List<CategoryAxis> xaxis) InitDataAndXaxis()
        {
            List<CategoryAxis> xaxis = new List<CategoryAxis>();
            List<ColumnSeries> data = new List<ColumnSeries>();

            int curOffset = 4, offset = 13;

            for (int i = 0; i < 4; ++i)
            {
                if (i < 3) { 
                    xaxis.Add(new CategoryAxis
                    {
                        Position = AxisPosition.Bottom,
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                        AxisTickToLabelDistance = curOffset
                    });
                    curOffset += offset;
                }

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

        public void SortBasePlot(string par)
        {
            plotView.Model.InvalidatePlot(true);
            plotView.Model.Series.Clear();
            plotView.Model.InvalidatePlot(true);
            plotView.Model.Axes.Clear();

            (var data, var xaxis) = InitDataAndXaxis();

            switch (par)
            {
                case "Кол-во процессоров":
                    compareList.List.Sort((Stat st1, Stat st2) => st1.Info.nproc - st2.Info.nproc);
                    break;
                case "Потерянное время":
                    compareList.List.Sort((Stat st1, Stat st2) =>
                         (int) (100 * (st1.Info.inter[0].times.lost_time - st2.Info.inter[0].times.lost_time)));
                    break;
                case "Время выполнения":
                    compareList.List.Sort((Stat st1, Stat st2) =>
                        (int)(100 * (st1.Info.inter[0].times.exec_time - st2.Info.inter[0].times.exec_time)));
                    break;
                case "Коэф. эффективности":
                    compareList.List.Sort((Stat st1, Stat st2) =>
                        (int)(100 * (st1.Info.inter[0].times.efficiency - st2.Info.inter[0].times.efficiency)));
                    break;
            }

            foreach (var item in compareList.List)
            {
                xaxis[0].Labels.Add("Кол-во процессоров: " + item.Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: " + item.Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: " + item.Info.inter[0].times.efficiency.ToString("F3"));
                data[0].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf_sys));
                data[1].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf_user));
                data[2].Items.Add(new ColumnItem(item.Info.inter[0].times.idle));
                data[3].Items.Add(new ColumnItem(item.Info.inter[0].times.comm));
            }

            plotView.Model.Axes.Add(yaxis);
            plotView.Model.InvalidatePlot(true);

            for (int i = 0; i < 3; ++i) {
                plotView.Model.Axes.Add(xaxis[i]);
                plotView.Model.InvalidatePlot(true);
                plotView.Model.Series.Add(data[i]);
                plotView.Model.InvalidatePlot(true);
            }
            baseModel.Series.Add(data[3]);
            plotView.Model.InvalidatePlot(true);

        }

        public void DetailPlot(StatCompareList compareList)
        {
            // TODO: Сделать что-то ?
        }

        public void IntervalComparePlot(StatCompareList compareList, int intervalNum, double maxTime)
        {
            intervalModel = new PlotModel();
            intervalModel.Title = "Потерянное время";

            //---  Init axis  ---//
            (var data, var xaxis) = InitDataAndXaxis();

            yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;
            if (maxTime > 0)
                yaxis.Maximum = maxTime;
            yaxis.Key = "Time";

            List<IntervalJson> intervals = new List<IntervalJson>();
            for (int i = 0; i < ViewController.CompareList.GetCount(); ++i)
                intervals.Add(ViewController.CompareList.At(i).Interval.GetIntervalAt(intervalNum).Info);

            for (int i = 0; i < compareList.GetCount(); ++i)
            {
                xaxis[0].Labels.Add("Кол-во процессоров: "
                    + compareList.At(i).Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: "
                    + compareList.At(i).Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: "
                    + compareList.At(i).Info.inter[0].times.efficiency.ToString("F3"));
                data[0].Items.Add(new ColumnItem(intervals[i].times.insuf_sys));
                data[1].Items.Add(new ColumnItem(intervals[i].times.insuf_user));
                data[2].Items.Add(new ColumnItem(intervals[i].times.idle));
                data[3].Items.Add(new ColumnItem(intervals[i].times.comm));
            }

            for (int i = 0; i < 3; ++i)
            {
                intervalModel.Axes.Add(xaxis[i]);
                intervalModel.Series.Add(data[i]);
            }
            intervalModel.Series.Add(data[3]);
            intervalModel.Axes.Add(yaxis);

            plotView.Model = intervalModel;
        }

    }
}
