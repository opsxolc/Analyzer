﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;

namespace Analyzer
{

    public partial class ViewController : NSViewController
    {
        private NSAlert Alert;  // диалоговое окно для сообщений
        private NSAlert RemoveAlert;
        public static StatCompareList CompareList;  // static для доступа с другого ViewController'a
        private nint YesButtonTag, NoButtonTag;
        public static NSStoryboard storyboard = NSStoryboard.FromName("Main", null);
        private NSWindowController popoverWindowController;
        private PopoverController popoverViewController;
        private double plotCompareMaxTime;
        private double plotStatMaxTime;
        private bool firstTime;
        private NSPopover helpPopover;
        public static Stat LoadedStat;

        public override void MouseDown(NSEvent theEvent)
        {
            PlotMaker.clickPoint = plotView.ConvertPointFromView(theEvent.LocationInWindow, null);
            base.MouseDown(theEvent);
        }

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        private void InitInterTree()
        {
            plotStatMaxTime = -1;
            InterTreeSplitView.SetPositionOfDivider(310, 0);
            InterTreeSplitView.SetPositionOfDivider(InterTreeSplitView.Frame.Width, 1);
            InterTreeSegmentController.SetSelected(false, 1);
            InterTreeSegmentController.SetSelected(true, 0);
        }

        // Добавить статистику в StatTableView
        public void AddStatToList(Stat stat, string creationTime)
        {
            ((StatTableDataSource)StatTableView.DataSource).StatDirs
                .Add(new StatDir(StatDir.StatDirPath + '/' + stat.GetHashCode()
                + "/stat.json", creationTime,
                stat.GetHashCode().ToString(), stat.GetInfoForStatDir())); 
            StatTableView.ReloadData();
        }

        public void LoadStatList()
        {
            Regex dirNameRgx = new Regex(@"-?[0-9]*$");
            var DataSource = new StatTableDataSource();
            var dirs = Directory.GetDirectories(StatDir.StatDirPath);
            string file, creationTime;
            foreach (string dir in dirs)
            {
                file = Directory.GetFiles(dir, "*.json")[0];
                creationTime = File.GetCreationTime(file).ToString();
                Stat stat = new Stat(file);
                DataSource.StatDirs.Add(new StatDir(file, creationTime,
                    dirNameRgx.Match(dir).Value, stat.GetInfoForStatDir()));
            }
            StatTableView.DataSource = DataSource;
            StatTableView.Delegate = new StatTableDelegate();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view.
            LoadStatList();
            InitDataInterView();
            InitInterTree();
            InterTreeSplitView.SetPositionOfDivider(0, 0);
            InterTreeSegmentController.SetSelected(false, 0);
            firstTime = true;

            //---  Init alerts  ---//
            Alert = new NSAlert();

            RemoveAlert = new NSAlert();
            NoButtonTag = RemoveAlert.AddButton("Нет").Tag;
            YesButtonTag = RemoveAlert.AddButton("Да").Tag;
            RemoveAlert.MessageText = "Вы уверены, что хотите " +
                "удалить выбранные статистики выполнения?";

            CompareList = new StatCompareList();
            plotView.Model = new OxyPlot.PlotModel();

            //---  Init popover  ---//
            popoverWindowController = storyboard.InstantiateControllerWithIdentifier("Popover") as NSWindowController;
            popoverViewController = popoverWindowController.ContentViewController as PopoverController;

            //---  Init help button  ---//
            NSButton helpIntervalCompare = new NSButton
            {
                BezelStyle = NSBezelStyle.HelpButton,
                Title = "",
                BezelColor = NSColor.White
            };
            helpIntervalCompare.SetButtonType(NSButtonType.MomentaryPushIn);
            helpIntervalCompare.SetFrameSize(helpIntervalCompare.FittingSize);
            helpIntervalCompare.SetFrameOrigin(new CGPoint(TableHeader.Frame.Width
                - helpIntervalCompare.FittingSize.Width, 2));
            helpIntervalCompare.AutoresizingMask = NSViewResizingMask.MinXMargin;

            //---  Init help popover  ---//
            var helpWindowController = storyboard.InstantiateControllerWithIdentifier("Popover") as NSWindowController;
            var helpViewController = helpWindowController.ContentViewController as PopoverController;
            helpPopover = new NSPopover
            {
                ContentSize = new CGSize(200, 180),
                Behavior = NSPopoverBehavior.Transient,
                Animates = true,
                ContentViewController = helpViewController
            };
            helpIntervalCompare.Activated += (object sender, EventArgs e)
                => helpPopover.Show(new CGRect(helpIntervalCompare.Frame.Location, new CGSize(200, 180)),
                   TableHeader, NSRectEdge.MaxYEdge);

            TableHeader.AddSubview(helpIntervalCompare);

            //---  Init Compare Controls  ---//
            CompareSort.Enabled = false;
            IntervalCompareButton.Enabled = false;
            CompareDiagramSelect.Enabled = false;
            CompareSort.RemoveAllItems();
            string[] CompareItems = { "Кол-во процессоров", "Потерянное время",
                "Время выполнения",  "Коэф. эффективности"};
            CompareSort.AddItems(CompareItems);

            CompareSort.Activated += (object sender, EventArgs e)
                =>
            {
                // TODO: Мб, для этого есть нормальное решение?
                var selectedRow = (int)CompareIntervalTree.SelectedRow;
                CompareList.Sort(CompareSort.TitleOfSelectedItem, selectedRow);
                switch (plotView.Model.Title)
                {
                    case "Потерянное время":
                        plotView.Model = PlotMaker.LostTimeComparePlot(selectedRow,
                            plotView.Model.GetAxis("Time").ActualMaximum);
                        break;
                    case "ГПУ":
                        plotView.Model = PlotMaker.GPUComparePlot(selectedRow,
                            plotView.Model.GetAxis("Time").ActualMaximum);
                        break;
                }
                CompareIntervalTree.ReloadData();
                CompareIntervalTree.SelectRow(selectedRow, false);
            };

            CompareIntervalTree.IntercellSpacing = new CGSize(10, 0);
            var dataSource = new IntervalOutlineDataSource();
            CompareIntervalTree.DataSource = dataSource;
            CompareIntervalTree.Delegate = new IntervalCompareOutlineDelegate(CompareIntervalTree, plotView);

            IntervalCompareButton.Activated += IntervalCompareButton_Activated;
        }

        private void IntervalCompareButton_Activated(object sender, EventArgs e)
        {
            if (IntervalCompareButton.State == NSCellStateValue.On)
                CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width - 200, 0);
            else {
                CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width, 0);
                CompareIntervalTree.SelectRow(0, false);
            }
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        private void InitDataInterView()
        {
            var DataSource = new IntervalOutlineDataSource();
            InterView.DataSource = DataSource;
            InterView.Delegate = new IntervalOutlineDelegate(DataSource, this);
        }

        public void InterView_SelectionDidChange(object sender, EventArgs e)
        {
            var item = InterView.ItemAtRow(InterView.SelectedRow) as Interval;
            InterText.Value = item.Text;
            InterTreePlotView.Model = PlotMaker.ProcLostTimePlot(LoadedStat, item.Row, plotStatMaxTime);
        }

        private void SetDataToInterView(Stat stat)
        {
            InitInterTree();
            InterText.Value = "";
            var intervals = ((IntervalOutlineDataSource)InterView.DataSource).Intervals;
            intervals.Clear();
            intervals.Add(stat.Interval);
            InterView.ReloadData();
            InterView.ExpandItem(InterView.ItemAtRow(0), true);
            InterView.SelectRow(0, false);
            plotStatMaxTime = InterTreePlotView.Model.GetAxis("Time").ActualMaximum;
        }

        partial void CloseButton(Foundation.NSObject sender)
        {
            Environment.Exit(0);
        }

        partial void ReadStat(Foundation.NSObject sender)
        { 
            string res;
            Stat stat;
            LibraryImport.read_stat_(StatPath.StringValue, out res);
            if (res == null) {
                Alert.MessageText = "Не найден файл \"" + StatPath.StringValue + "\".";
                Alert.RunModal();
                return;
            }
            // Директория с загружаемым файлом
            string TmpFileLocDir = Path.GetDirectoryName(StatPath.StringValue); 
            Console.WriteLine(res);
            stat = new Stat(res, TmpFileLocDir);

            //---  Save files  ---//
            string TmpDirPath = StatDir.StatDirPath + stat.GetHashCode();
            Directory.CreateDirectory(TmpDirPath);
            string creationTime = DateTime.Now.ToString();
            FileStream file = File.Create(TmpDirPath + "/stat.json");
            file.Write(new UTF8Encoding(true).GetBytes(res));
            file.Close();
            foreach (var inter in stat.Info.inter)
            {
                if (!File.Exists(TmpDirPath + '/' + inter.id.pname))
                    try
                    {
                        if (File.Exists(TmpFileLocDir + '/' + inter.id.pname))
                            File.Copy(TmpFileLocDir + '/' + inter.id.pname,
                                TmpDirPath + '/' + inter.id.pname);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not copy file '" + inter.id.pname + "'");
                    }
            }
            AddStatToList(stat, creationTime);
        }

        partial void LoadStat(NSObject sender)
        {

            if (StatTableView.SelectedRowCount != 1) {
                Alert.MessageText = "Пожалуйста, выберите одну " +
                    "запись для загрузки.";
                Alert.RunModal();
                return;
            }

            StatDir statDir = ((StatTableDataSource)StatTableView.DataSource)
                .StatDirs[(int)StatTableView.SelectedRow];
            LoadedStat = new Stat(statDir.path);
            SetDataToInterView(LoadedStat);
            firstTime = false;

            //---  Switch active view   ---//
            TabView.SelectAt(1);
        }

        partial void DeleteStat(NSObject sender)
        {
            if (StatTableView.SelectedRowCount == 0 || RemoveAlert.RunModal() != YesButtonTag)
                return;
            var dataSource = (StatTableDataSource)StatTableView.DataSource;
            var selectedRows = StatTableView.SelectedRows.ToArray();
            for (int i = selectedRows.Length - 1; i >=0; --i) { 
                StatDir statDir = dataSource.StatDirs[(int)selectedRows[i]];
                Directory.Delete(StatDir.StatDirPath + '/' + statDir.hash, true);
                ((StatTableDataSource)StatTableView.DataSource).StatDirs.Remove(statDir);
            }
            StatTableView.RemoveRows(StatTableView.SelectedRows, NSTableViewAnimation.Fade);
        }

        partial void CompareStat(NSObject sender)
        {
            var SelectedRowsArray = StatTableView.SelectedRows.ToArray();
            if (SelectedRowsArray.Length < 2)
            { 
                Alert.MessageText = "Пожалуйста, выберите " +
                    "2 или более записи для сравнения.";
                Alert.RunModal();
                return;
            }
            plotView.Model = null;
            CompareList.Clear();
            var StatDirs = ((StatTableDataSource)StatTableView.DataSource).StatDirs;

            try { 
                foreach (var i in SelectedRowsArray)
                    CompareList.Add(new Stat(StatDirs[(int)i].ReadJson(),
                        Path.GetDirectoryName(StatDirs[(int)i].path)));
            } catch (Exception e)
            {
                Console.WriteLine("Got one!\n" + e);
                CompareList.Clear();
                foreach (var i in SelectedRowsArray)
                    CompareList.Add(new Stat(StatDirs[(int)i].ReadJson(),
                        Path.GetDirectoryName(StatDirs[(int)i].path)));
            }
            CompareList.BuildIntervalsList();

            //---  Hide label and set plot  ---//
            ChooseLabel.Hidden = true;
            CompareSort.Enabled = true;
            CompareDiagramSelect.Enabled = true;
            IntervalCompareButton.Enabled = true;
            plotView.Model = PlotMaker.LostTimeComparePlot();
            plotCompareMaxTime = plotView.Model.GetAxis("Time").ActualMaximum;
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).maxTimeLost = plotCompareMaxTime;

            //---  Set CompareIntervalTree  ---//
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals.Clear();
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals
                .Add(CompareList.List[0].Interval);
            CompareIntervalTree.ReloadData();
            CompareIntervalTree.ExpandItem(CompareIntervalTree.ItemAtRow(0), true);
            CompareIntervalTree.SelectRow(0, false);

            //--- Switch tab  ---//
            TabView.SelectAt(2);
            if (firstTime)
            {
                CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width - 200, 0);
                firstTime = false;
            }

        }

        partial void CompareReset(NSObject sender)
        {
            plotView.Model = null;
            CompareSort.Enabled = false;
            CompareDiagramSelect.Enabled = false;
            CompareDiagramSelect.SelectItem(0);
            CompareSort.SelectItem(0);
            ChooseLabel.Hidden = false;
            IntervalCompareButton.Enabled = false;
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).maxTimeLost = -1;
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).maxTimeGPU = -1;
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals.Clear();
            CompareIntervalTree.ReloadData();
            CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width, 0);
        }

        partial void CompareBack(NSObject sender)
        {
            //TODO: Сделать нормальный "Назад", наверно

            NSPopover popover = new NSPopover();
            popover.ContentSize = new CGSize(100, 100);
            popover.Behavior = NSPopoverBehavior.Transient;
            popover.Animates = true;
            popover.ContentViewController = popoverViewController;

            popover.Show(new CGRect(new CGPoint(0, 0), new CGSize(100, 100)), View, NSRectEdge.MaxXEdge);
        }

        partial void InterTreeSegment(NSObject sender)
        {
            var s = sender as NSSegmentedControl;

            if (s.IsSelectedForSegment(0) && InterTreePlotView.Frame.Width <= 20)
                InterTreeSplitView.SetPositionOfDivider(310, 0);
            else if (!s.IsSelectedForSegment(0))
                InterTreeSplitView.SetPositionOfDivider(0, 0);
            
            if (s.IsSelectedForSegment(1) && InterText.Frame.Width <= 20)
            {
                NSAnimationContext.RunAnimation((NSAnimationContext a) => {
                    a.AllowsImplicitAnimation = true;
                    a.Duration = 0.5;
                    InterTreeSplitView.SetPositionOfDivider(InterTreeSplitView.Frame.Width - 300, 1);
                });
            }
            else if (!s.IsSelectedForSegment(1))
                NSAnimationContext.RunAnimation((NSAnimationContext a) => {
                    a.AllowsImplicitAnimation = true;
                    a.Duration = 0.5;
                    InterTreeSplitView.SetPositionOfDivider(InterTreeSplitView.Frame.Width, 1);
                });

        }

        partial void CompareDiagramSelect_Activated(NSObject sender)
        {
            var s = sender as NSPopUpButton;
            double newMax;
            switch (s.TitleOfSelectedItem)
            {
                case "Потерянное время ЦПУ":
                    if (plotView.Model.Title == "Потерянное время")
                        return;
                    plotView.Model = PlotMaker.LostTimeComparePlot();
                    newMax = plotView.Model.GetAxis("Time").ActualMaximum;
                    ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).maxTimeLost = newMax;
                    Console.WriteLine("CPU NewMax = " + newMax);
                    plotView.Model = PlotMaker.LostTimeComparePlot((int)CompareIntervalTree.SelectedRow, newMax);
                    break;
                case "Сравнение ГПУ":
                    if (plotView.Model.Title == "ГПУ")
                        return;
                    plotView.Model = PlotMaker.GPUComparePlot();
                    newMax = plotView.Model.GetAxis("Time").ActualMaximum;
                    ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).maxTimeGPU = newMax;
                    Console.WriteLine("GPU NewMax = " + newMax);
                    plotView.Model = PlotMaker.GPUComparePlot((int)CompareIntervalTree.SelectedRow, newMax);
                    break;
            }
        }

        partial void StatGPUButton_Activated(NSObject sender)
        {
            //GPUStackView.AddView(new GPUView(), NSStackViewGravity.Top);
        }

    }


}