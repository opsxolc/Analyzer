using System;
namespace Analyzer
{
    public class StatDir
    {
        public static string StatDirPath = @"/Users/penek/Projects/Analyzer/StatDir/";

        public string path;
        public string creationTime;
        public string hash;
        public string info;

        public StatDir() { }

        public StatDir(string path, string creationTime, string hash, string info)
        {
            this.path = path;
            this.creationTime = creationTime;
            this.hash = hash;
            this.info = info;
        }
    }
}
