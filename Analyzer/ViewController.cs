using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AppKit;
using Foundation;

namespace Analyzer
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

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
            if (res == null)
                return;  // TODO: show dialog box "Could not find file by path: "<path>"."
            string TmpFileLocDir = Path.GetDirectoryName(StatPath.StringValue); // Директория с загружаемым файлом
            Console.WriteLine(res);
            stat = new Stat(res, TmpFileLocDir);

            //---  Save files  ---//
            string TmpDirPath = StatDir.StatDirPath + stat.GetHashCode();
            Directory.CreateDirectory(TmpDirPath);
            string creationTime = DateTime.Now.ToString();
            FileStream file = File.Create(TmpDirPath + "/stat.json");
            file.Write(new UTF8Encoding(true).GetBytes(res));
            file.Close();
            File.Copy(TmpFileLocDir + '/' + stat.Info.inter[0].id.pname,
                TmpDirPath + '/' + stat.Info.inter[0].id.pname);
            AddStatToList(stat, creationTime);

            //---  Set data to InterView  ---//
            SetDataToInterView(stat);
        }

        partial void LoadStat(NSObject sender)
        {
            if (!StatTableView.IsRowSelected(StatTableView.SelectedRow))
                return;
            StatDir statDir = ((StatTableDataSource)StatTableView.DataSource)
                .StatDirs[(int)StatTableView.SelectedRow];
            SetDataToInterView(new Stat(statDir.path));
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

    }


}
