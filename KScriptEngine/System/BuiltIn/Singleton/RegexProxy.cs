using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;

namespace KScript.KSystem
{
    public static class RegexProxy
    {
        [MemberMap("test", MapModifier.Static, MapType.Method)]
        public static double Test(KString text, KString pattern)
        {
            var match = Regex.Match(text, pattern);
            return Convert.ToDouble(match != null && match.Length == text.Length);
        }

        public static double Test(KString text, KRegex pattern)
        {
            return Convert.ToDouble(pattern.Test(text));
        }

        [MemberMap("matches", MapModifier.Static, MapType.Method)]
        public static KTuple Matches(KString text, KString pattern)
        {
            List<KString> res = new List<KString>(16);
            foreach (Match match in Regex.Matches(text, pattern))
            {
                res.Add(match.Value);
            }
            return new KTuple(res.ToArray());
        }

        //[FuncMap("group")]
        //public static List<KString> Group(double index)
        //{
        //    List<KString> res = new List<KString>(16);
        //    foreach (Match match in Regex.Matches(text, pattern))
        //    {
        //        res.Add(match.Value);
        //    }
        //    return res;
        //}
    }
}
