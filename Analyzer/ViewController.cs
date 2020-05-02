using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AppKit;
using CoreGraphics;
using Foundation;

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
        public double plotCompareMaxTime;
        public double plotStatMaxTime;
        private bool firstTime;
        private NSPopover helpPopover;
        public static Stat LoadedStat;
        public static int SelectedProc = -1;

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
            DeselectProcessors();
            plotStatMaxTime = -1;
            InterTreeSplitView.SetPositionOfDivider(510, 0);
            InterTreeSplitView.SetPositionOfDivider(InterTreeSplitView.Frame.Width, 1);
            InterTreeSegmentController.SetSelected(false, 1);
            InterTreeSegmentController.SetSelected(true, 0);
        }

        public void SelectTab(int index)
        {
            TabView.SelectAt(index);
        }

        // Добавить статистику в StatTableView
        public void AddStatToList(Stat stat, DateTime creationTime)
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
            string file;
            DateTime creationTime;
            foreach (string dir in dirs)
            {
                file = Directory.GetFiles(dir, "*.json")[0];
                creationTime = File.GetCreationTime(file);
                Stat stat = new Stat(file);
                DataSource.StatDirs.Add(new StatDir(file, creationTime,
                    dirNameRgx.Match(dir).Value, stat.GetInfoForStatDir()));
            }
            DataSource.StatDirs.Sort((StatDir x, StatDir y)
                => DateTime.Compare(x.creationTime, y.creationTime));
            StatTableView.DataSource = DataSource;
            StatTableView.Delegate = new StatTableDelegate();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view.
            GPUScrollView.DocumentView = GPUStackView;
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
            InterTreePlotView.Model = PlotMaker.ProcLostTimePlot(LoadedStat, this, item.Row, plotStatMaxTime);
            if (SelectedProc >= 0)
                SelectProcessor(SelectedProc, item.Row);
        }

        public void SetDataToInterView(Stat stat, int row = 0)
        {
            LoadedStat = stat;
            InitInterTree();
            InterTreePlotView.Model = PlotMaker.ProcLostTimePlot(LoadedStat, this, 0, plotStatMaxTime);
            plotStatMaxTime = InterTreePlotView.Model.GetAxis("Time").ActualMaximum;
            InterText.Value = "";
            var intervals = ((IntervalOutlineDataSource)InterView.DataSource).Intervals;
            intervals.Clear();
            intervals.Add(stat.Interval);
            InterView.ReloadData();
            InterView.ExpandItem(InterView.ItemAtRow(0), true);
            InterView.SelectRow(row, false);
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
            try
            { 
                stat = new Stat(res, TmpFileLocDir);
            }
            catch (Exception)
            {
                LibraryImport.read_stat_(StatPath.StringValue, out res);
                stat = new Stat(res, TmpFileLocDir);
            }

            //---  Save files  ---//
            string TmpDirPath = StatDir.StatDirPath + stat.GetHashCode();
            Directory.CreateDirectory(TmpDirPath);
            var creationTime = DateTime.Now;
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

            DeselectProcessors();
            StatDir statDir = ((StatTableDataSource)StatTableView.DataSource)
                .StatDirs[(int)StatTableView.SelectedRow];
            SetDataToInterView(new Stat(statDir.path, true));
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
            CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width - 300, 0);
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals.Clear();
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals
                .Add(CompareList.List[0].Interval);
            CompareIntervalTree.ReloadData();
            CompareIntervalTree.ExpandItem(CompareIntervalTree.ItemAtRow(0), true);
            CompareIntervalTree.SelectRow(0, false);
            CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width, 0);
            IntervalCompareButton.State = NSCellStateValue.Off;

            //--- Switch tab  ---//
            TabView.SelectAt(2);
        }

        partial void CompareReset(NSObject sender)
        {
            plotView.Model = null;
            CompareSort.Enabled = false;
            CompareDiagramSelect.Enabled = false;
            IntervalCompareButton.Enabled = false;
            IntervalCompareButton.State = NSCellStateValue.Off;
            CompareDiagramSelect.SelectItem(0);
            CompareSort.SelectItem(0);
            ChooseLabel.Hidden = false;
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
                InterTreeSplitView.SetPositionOfDivider(510, 0);
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

        private void ClearGPUStackView()
        {
            for (int i = GPUStackView.Views.Length - 1; i >= 0; --i)
                GPUStackView.Views[i].RemoveFromSuperview();
        }

        public void SelectProcessor(int procNum, int intervalNum)
        {
            SelectedProc = procNum;
            ClearGPUStackView();
            var inter = LoadedStat.Info.inter[intervalNum];

            //---  If there is no GPU return  ---//
            if (inter.proc_times[procNum].gpu_times == null)
                return;

            //---  Init GPU-cards  ---//
            nfloat maxSize = 0;
            int i = 1;
            foreach (var gpu in inter.proc_times[procNum].gpu_times)
            {
                var GPUCardController = storyboard.InstantiateControllerWithIdentifier("GPUId") as GPUViewController;
                GPUCardController.LoadView();
                GPUCardController.Init(gpu, i++);
                GPUStackView.AddView(GPUCardController.View, NSStackViewGravity.Top);
                if (GPUCardController.Height > maxSize)
                    maxSize = GPUCardController.Height;
            }

            GPUStackView.SetFrameSize(new CGSize(675, maxSize * inter.proc_times[procNum].num_gpu + 5));

            if (GPUScrollView.Frame.Height < 10)
                PlotSplitView.SetPositionOfDivider(PlotSplitView.Frame.Height - 250, 0);
            //TODO: Скроллить наверх
            //CGPoint newScrollOrigin;
            //if (GPUScrollView.ContentView.IsFlipped)
            //    newScrollOrigin = new CGPoint(0.0, 0.0);
            //else
            //    newScrollOrigin = new CGPoint(0.0, (GPUScrollView.DocumentView as NSView).Frame.Height -
            //                  GPUScrollView.Frame.Height/2 - 50);
            //    //GPUScrollView.ContentView.ScrollToPoint(new CGPoint(0, maxSize * inter.proc_times[procNum].num_gpu - maxSize));
            //GPUScrollView.ContentView.ScrollToPoint(newScrollOrigin);
        }

        public void DeselectProcessors()
        {
            SelectedProc = -1;
            ClearGPUStackView();
            PlotSplitView.SetPositionOfDivider(PlotSplitView.Frame.Height, 0);
        }

        partial void ResetLoadStat(NSObject sender)
        {
            DeselectProcessors();
            LoadedStat = null;
            InterTreePlotView.Model = new OxyPlot.PlotModel();
            ((IntervalOutlineDataSource)InterView.DataSource).Intervals.Clear();
            InterView.ReloadData();
            InterTreeSplitView.SetPositionOfDivider(0, 0);
            InterTreeSegmentController.SetSelected(false, 0);
            InterTreeSplitView.SetPositionOfDivider(InterTreeSplitView.Frame.Width, 1);
            InterTreeSegmentController.SetSelected(false, 1);
            InterText.Value = "";
        }
    }


}