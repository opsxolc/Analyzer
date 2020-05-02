using System;
using System.IO;

namespace Analyzer
{
    public class StatDir
    {
        public static string StatDirPath = @"/Users/penek/Projects/Analyzer/StatDir/";

        public string path;
        public DateTime creationTime;
        public string hash;
        public string info;

        public StatDir() { }

        public StatDir(string path, DateTime creationTime, string hash, string info)
        {
            this.path = path;
            this.creationTime = creationTime;
            this.hash = hash;
            this.info = info;
        }

        public string ReadJson()
        {
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();
            return json;
        }
    }
}
