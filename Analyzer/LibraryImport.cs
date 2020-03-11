using System;
using System.Runtime.InteropServices;

namespace Analyzer
{
    public class LibraryImport
    {
        [DllImport("integration.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int read_stat_(string name, out string res);

        [DllImport("integration.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int stat_intersect_(string st1, string st2, out string res1, out string res2);
    }
}