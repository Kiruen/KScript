using KScript.Runtime;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem
{
    public static class FileProxy
    {
        [MemberMap("read", MapModifier.Static, MapType.Method)]
        public static string ReadFile(KString path)
        {
            if(File.Exists(path))
                return File.ReadAllText(path);
            throw new KException("File not found!", Debugger.CurrLineNo);
        }

        [MemberMap("write", MapModifier.Static, MapType.Method)]
        public static void WriteFile(KString path, KString text, KString mod)
        {
            string mode = mod;
            mode = mode.ToLower().Trim();
            if (mode.StartsWith("ov"))
                File.WriteAllText(path, text);
            else if (mode.StartsWith("ap"))
                File.AppendAllText(path, text);

        }
    }
}
