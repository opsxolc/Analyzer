using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AppKit;
using Foundation;
using System.Drawing;
using System.Data;

namespace Analyzer
{
    public partial class ViewController : NSViewController
    {
        private NSAlert Alert;  // диалоговое окно для сообщений
        private StatCompareList CompareList;

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
            CompareList = new StatCompareList();
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
            TabView.SelectLast(TabView);
        }

        partial void DeleteStat(NSObject sender)
        {
            if (!StatTableView.IsRowSelected(StatTableView.SelectedRow))
                return;
            var dataSource = (StatTableDataSource)StatTableView.DataSource;
            StatDir statDir = dataSource.StatDirs[(int)StatTableView.SelectedRow];
            Console.WriteLine("Delete: " + StatTableView.SelectedRow + statDir.hash);
            ((StatTableDataSource)StatTableView.DataSource).StatDirs.Remove(statDir);
            StatTableView.ReloadData();
            Directory.Delete(StatDir.StatDirPath + '/' + statDir.hash, true);
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

            Console.WriteLine(CompareList.GetCount() + "\n\n" + CompareList.At(0).ToJson());

        }

    }


}