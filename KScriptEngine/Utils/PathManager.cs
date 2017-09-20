using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KScript.Utils
{
    public static class KPath
    {
        static KPath()
        {
            EngineRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Startup = AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string Startup { get; set; }

        public static string LibRoot
        {
            get { return $"{Path.GetDirectoryName(EngineRoot)}\\lib"; }
        }

        public static string MoudleRoot
        {
            get { return $"{Path.GetDirectoryName(EngineRoot)}\\module"; }
        }

        public static string EngineRoot { get; private set; }

        public static string Current
        {
            get { return Assembly.GetCallingAssembly().Location; }
        }

        public static string GetModuleName(string url)
        {
            if (url.Contains('.'))
            {
                var temp = url.Split('.');
                return temp[temp.Length - 2];
            }
            else return url;
        }
    }
}
