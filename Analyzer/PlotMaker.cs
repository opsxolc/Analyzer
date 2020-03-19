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

        private PlotView plotView;
        private StatCompareList compareList;
        private PlotModel baseModel;
        private PlotModel detailModel;
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
            
            (var data, var xaxis) = InitDataAndXaxis();
            
            yaxis = new LinearAxis();
            yaxis.Position = AxisPosition.Left;
            yaxis.MajorGridlineStyle = LineStyle.Dot;
            yaxis.MinorGridlineStyle = LineStyle.Dot;
            yaxis.Title = "Время, с";
            yaxis.AxisTitleDistance = 7;
            yaxis.AbsoluteMinimum = 0;

            foreach (var item in compareList.List)
            {
                xaxis[0].Labels.Add("Кол-во процессоров: " + item.Info.nproc);
                xaxis[1].Labels.Add("Время выполнения: " + item.Info.inter[0].times.exec_time.ToString("F3"));
                xaxis[2].Labels.Add("Коэфф. эффективности: " + item.Info.inter[0].times.efficiency.ToString("F3"));
                data[0].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf));
                data[1].Items.Add(new ColumnItem(item.Info.inter[0].times.idle));
                data[2].Items.Add(new ColumnItem(item.Info.inter[0].times.comm));
            }

            baseModel = new PlotModel();

            baseModel.Title = "Потерянное время";
            for (int i = 0; i < 3; ++i)
            {
                baseModel.Axes.Add(xaxis[i]);
                baseModel.Series.Add(data[i]);
            }
            baseModel.Axes.Add(yaxis);

            plotView.Model = baseModel;
        }

        private (List<ColumnSeries> data, List<CategoryAxis> xaxis) InitDataAndXaxis()
        {
            List<CategoryAxis> xaxis = new List<CategoryAxis>();
            List<ColumnSeries> data = new List<ColumnSeries>();

            int curOffset = 4, offset = 13;

            for (int i = 0; i < 3; ++i)
            {
                xaxis.Add(new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Angle = 5,
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
            data[0].Title = "Недостаточный параллелизм";
            data[0].MouseDown += (object sender, OxyMouseDownEventArgs e)
                => Console.WriteLine("data[0] MouseDown "
                + e.ClickCount + " " + e.Position);

            data[1].FillColor = OxyColors.LightSkyBlue;
            data[1].Title = "Простои";

            data[2].FillColor = OxyColors.GreenYellow;
            data[2].Title = "Коммуникации";

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
                data[0].Items.Add(new ColumnItem(item.Info.inter[0].times.insuf));
                data[1].Items.Add(new ColumnItem(item.Info.inter[0].times.idle));
                data[2].Items.Add(new ColumnItem(item.Info.inter[0].times.comm));
            }

            plotView.Model.Axes.Add(yaxis);
            plotView.Model.InvalidatePlot(true);

            for (int i = 0; i < 3; ++i) {
                plotView.Model.Axes.Add(xaxis[i]);
                plotView.Model.InvalidatePlot(true);
                plotView.Model.Series.Add(data[i]);
                plotView.Model.InvalidatePlot(true);
            }

        }

        public void DetailPlot(StatCompareList compareList)
        {

        }

    }
}
