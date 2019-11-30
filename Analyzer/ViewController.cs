using System;
using AppKit;
using Foundation;

namespace Analyzer
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib() { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view.
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

        partial void CloseButton(Foundation.NSObject sender)
        {
            Environment.Exit(0);
        }

        partial void ReadStat(Foundation.NSObject sender)
        {
            string res;
            StatJson stat;
            LibraryImport.read_stat_(StatPath.StringValue, out res);
            if (res == null)
                return;
            stat = UseStatJson.GetStat(res);
            Text.Value = stat.nproc + " " + stat.iscomp + " " + stat.proc[0]
                .node_name + " " + stat.proc[0].test_time;
            var inter = new Interval(stat.inter);
            var DataSource = new IntervalOutlineDataSource();
            DataSource.Intervals.Add(inter);
            InterView.DataSource = DataSource;
            InterView.Delegate = new IntervalOutlineDelegate(DataSource);

            Text.Value += "\n" + inter;
            //Text.Value += "\nInfo: " + inter.Info.id.nline + " to " +
            //    inter.Info.id.nline_end;

        }

    }


}
