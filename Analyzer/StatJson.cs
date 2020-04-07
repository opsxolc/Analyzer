using Newtonsoft.Json;
using System.Collections.Generic;

namespace Analyzer
{

    public class UseStatJson
    {
        public static StatJson GetStat(string json) => JsonConvert.DeserializeObject<StatJson>(json);
        public static string GetJson(StatJson stat) => JsonConvert.SerializeObject(stat);
    }

    public struct StatJson
    {
        public int nproc;
        public bool iscomp;
        public List<ProcInfoJson> proc;
        public List<IntervalJson> inter;
    }

    public struct ProcInfoJson
    {
        public string node_name;
        public double test_time;
    }

    public struct IntervalJson
    {
        public IdentJson id;
        public InterTimesJson times;
    }

    public struct IdentJson
    {
        public int nlev;
        public int t;
        public int expr;
        public int nline;
        public int nline_end;
        public double nenter;
        public string pname;
    }

    public struct InterTimesJson
    {
        public double prod_cpu;
        public double prod_sys;
        public double prod_io;
        public double prod;
        public double exec_time;
        public double sys_time;
        public double efficiency;
        public double lost_time;
        public double insuf;
        public double insuf_user;
        public double insuf_sys;
        public double comm;
        public double real_comm;
        public double comm_start;
        public double idle;
        public double load_imb;
        public double synch;
        public double time_var;
        public double overlap;
        public double thr_user_time;
        public double thr_sys_time;
        public double gpu_time_prod;
        public double gpu_time_lost;
        public long nproc;
        public long threadsOfAllProcs;
    
    }
}
