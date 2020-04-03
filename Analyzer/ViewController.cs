using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AppKit;
using Foundation;

namespace Analyzer
{

    public partial class ViewController : NSViewController
    {
        private NSAlert Alert;  // диалоговое окно для сообщений
        private NSAlert RemoveAlert;
        public static StatCompareList CompareList;  // static для доступа с другого ViewController'a
        private nint YesButtonTag, NoButtonTag;
        private NSWindowController IntervalCompareController;
        private static NSStoryboard storyboard = NSStoryboard.FromName("Main", null);
        private double plotMaxTime;
        private bool firstTime;

        public ViewController(IntPtr handle) : base(handle)
        {
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

        public void Selected(string text)
        {
            InterText.Value = text;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view.
            LoadStatList();
            InitDataInterView();
            firstTime = true;
            Alert = new NSAlert();

            RemoveAlert = new NSAlert();
            NoButtonTag = RemoveAlert.AddButton("Нет").Tag;
            YesButtonTag = RemoveAlert.AddButton("Да").Tag;
            RemoveAlert.MessageText = "Вы уверены, что хотите " +
                "удалить выбранные статистики выполнения?";

            CompareList = new StatCompareList();
            plotView.Model = new OxyPlot.PlotModel();

            
            CompareSort.Enabled = false;
            IntervalCompareButton.Enabled = false;
            CompareSort.RemoveAllItems();
            string[] CompareItems = { "Кол-во процессоров", "Потерянное время",
                "Время выполнения",  "Коэф. эффективности"};
            CompareSort.AddItems(CompareItems);

            CompareSort.Activated += (object sender, EventArgs e)
                => PlotMaker.SortPlot(plotView, CompareSort.TitleOfSelectedItem,
                    (int)CompareIntervalTree.SelectedRow);

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
            InterView.Delegate = new IntervalOutlineDelegate(DataSource, this, InterView);
        }

        private void SetDataToInterView(Stat stat)
        {
            InterText.Value = "";
            var intervals = ((IntervalOutlineDataSource)InterView.DataSource).Intervals;
            intervals.Clear();
            intervals.Add(stat.Interval);
            InterView.ReloadData();
            InterView.ExpandItem(InterView.ItemAtRow(0), true);
        }

        private void InterView_DoubleClick(object sender, EventArgs e)
        {
            Console.WriteLine("Hello");
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
            try { 
                File.Copy(TmpFileLocDir + '/' + stat.Info.inter[0].id.pname,
                    TmpDirPath + '/' + stat.Info.inter[0].id.pname);
            } catch (Exception) {
                Console.WriteLine("Could not open file");
            }
            AddStatToList(stat, creationTime);

            //---  Set data to InterView  ---//
            SetDataToInterView(stat);
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
            SetDataToInterView(new Stat(statDir.path));

            //---  Switch active view   ---//
            TabView.SelectAt(1);
        }

        partial void DeleteStat(NSObject sender)
        {
            if (StatTableView.SelectedRowCount == 0 || RemoveAlert.RunModal() != YesButtonTag)
                return;
            var dataSource = (StatTableDataSource)StatTableView.DataSource;
            foreach (var row in StatTableView.SelectedRows) { 
                StatDir statDir = dataSource.StatDirs[(int)row];
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

            foreach (var i in SelectedRowsArray)
                CompareList.Add(new Stat(StatDirs[(int)i].ReadJson(),
                    Path.GetDirectoryName(StatDirs[(int)i].path)));

            //---  Set CompareIntervalTree  ---//
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).SetMaxTime(-1);
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals.Clear();
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals
                .Add(CompareList.List[0].Interval);
            CompareIntervalTree.ReloadData();
            CompareIntervalTree.ExpandItem(CompareIntervalTree.ItemAtRow(0), true);
            CompareIntervalTree.SelectRow(0, false);

            //---  Hide label and set plot  ---//
            ChooseLabel.Hidden = true;
            CompareSort.Enabled = true;
            IntervalCompareButton.Enabled = true;

            plotMaxTime = plotView.Model.GetAxis("Time").ActualMaximum;
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).SetMaxTime(plotMaxTime);

            //--- Switch tab  ---//
            TabView.SelectAt(2);
            if (firstTime) { 
                CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width - 200, 0);
                firstTime = false;
            }

        }

        partial void CompareReset(NSObject sender)
        {
            plotView.Model = null;
            CompareSort.Enabled = false;
            ChooseLabel.Hidden = false;
            IntervalCompareButton.Enabled = false;
            ((IntervalCompareOutlineDelegate)CompareIntervalTree.Delegate).SetMaxTime(-1);
            ((IntervalOutlineDataSource)CompareIntervalTree.DataSource).Intervals.Clear();
            CompareIntervalTree.ReloadData();
            CompareSplitView.SetPositionOfDivider(CompareSplitView.Frame.Width, 0);
        }

        partial void CompareBack(NSObject sender)
        {
            //TODO: Сделать нормальный "Назад", наверно
        }

    }


}