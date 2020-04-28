using Newtonsoft.Json;
using System.Collections.Generic;

namespace Analyzer
{

    enum GPUOp
    {
        GSHADOW,
        GREMOTE,
        GREDISTRIBUTION,
        GREGIONIN,
        GNUMOP
    };

    public class UseStatJson
    {
        public static StatJson GetStat(string json) => JsonConvert.DeserializeObject<StatJson>(json);
        public static string GetJson(StatJson stat) => JsonConvert.SerializeObject(stat);
    }

    // TODO: Получать попроцессорные значения
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
        public List<ColOpJson> col_op;
        public List<ProcTimesJson> proc_times;
    }

    public struct ProcTimesJson
    {
        public double prod_cpu;
        public double prod_sys;
        public double prod_io;
        public double exec_time;
        public double sys_time;
        public double real_comm;
        public double lost_time;
        public double insuf_user;
        public double insuf_sys;
        public double comm;
        public double idle;
        public double load_imb;
        public double synch;
        public double time_var;
        public double overlap;
        public double thr_user_time;
        public double thr_sys_time;
        public double gpu_time_prod;
        public double gpu_time_lost;
        public ulong num_threads;
        public ulong num_gpu;
        public List<ThTimesJson> th_times;
        public List<GPUTimesJson> gpu_times;
    }

    public struct GPUTimesJson
    {
        public string gpu_name;
        public double prod_time;
        public double kernel_exec;
        public double loop_exec;
        public double lost_time;
        public double get_actual;
        public double data_reorg;
        public double reduction;
        public double gpu_runtime_compilation;
        public double gpu_to_cpu;
        public double cpu_to_gpu;
        public double gpu_to_gpu;
        public List<OpTimesJson> op_times;
    }

    public struct OpTimesJson
    {
        public double cpu_to_gpu;
        public double gpu_to_gpu;
        public double gpu_to_cpu;
    }

    public struct ThTimesJson
    {
        public double sys_time;
        public double user_time;
    }

    public struct ColOpJson
    {
        public double ncall;
        public double comm;
        public double real_comm;
        public double synch;
        public double time_var;
        public double overlap;
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
