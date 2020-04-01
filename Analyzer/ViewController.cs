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
        private PlotMaker PlotMaker;
        private nint YesButtonTag, NoButtonTag;
        private NSWindowController IntervalCompareController;
        private static NSStoryboard storyboard = NSStoryboard.FromName("Main", null);

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

            Alert = new NSAlert();

            RemoveAlert = new NSAlert();
            NoButtonTag = RemoveAlert.AddButton("Нет").Tag;
            YesButtonTag = RemoveAlert.AddButton("Да").Tag;
            RemoveAlert.MessageText = "Вы уверены, что хотите " +
                "удалить выбранные статистики выполнения?";

            CompareList = new StatCompareList();
            PlotMaker = new PlotMaker(plotView);

            IntervalCompareButton.Enabled = false;
            CompareSort.Enabled = false;
            CompareSort.RemoveAllItems();
            string[] CompareItems = { "Кол-во процессоров", "Потерянное время",
                "Время выполнения",  "Коэф. эффективности"};
            CompareSort.AddItems(CompareItems);

            CompareSort.Activated += (object sender, EventArgs e)
                => PlotMaker.SortBasePlot(CompareSort.TitleOfSelectedItem);

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
            CompareList.Clear();
            var StatDirs = ((StatTableDataSource)StatTableView.DataSource).StatDirs;

            for (int i = 0; i < SelectedRowsArray.Length; ++i)
                CompareList.Add(new Stat(StatDirs[(int)SelectedRowsArray[i]].ReadJson(),
                    Path.GetDirectoryName(StatDirs[(int)SelectedRowsArray[i]].path)));

            //---  Create controllet for interval comparison  ---//
            IntervalCompareController = storyboard
                .InstantiateControllerWithIdentifier("IntervalCompare") as NSWindowController;
            IntervalCompareController.Window.Title = "Поинтервальное сравнение";

            //---  Hide label and set plot  ---//
            ChooseLabel.Hidden = true;
            CompareSort.Enabled = true;
            IntervalCompareButton.Enabled = true;
            PlotMaker.BasePlot(CompareList);

            //--- Switch tab  ---//
            TabView.SelectAt(2);
        }

        partial void CompareReset(NSObject sender)
        {
            PlotMaker.ResetPlot();
            CompareSort.Enabled = false;
            IntervalCompareButton.Enabled = false;
            ChooseLabel.Hidden = false;
        }

        partial void CompareBack(NSObject sender)
        {
            //TODO: Сделать нормальный "Назад", наверно
        }


        partial void IntervalCompare(NSObject sender)
        {
            IntervalCompareController.ShowWindow(sender);
        }

    }


}