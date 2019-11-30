using System;
using System.Runtime.InteropServices;

namespace Analyzer
{
    public class LibraryImport
    {
        [DllImport("integration.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int read_stat_(string name, out string res);
    }
}