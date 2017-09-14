using KScript.KAttribute;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("Regex", MapModifier.Static, MapType.CommonClass)]
    public class KRegex : KBuiltIn
    {
        public string Pattern { get; private set; }
        public Regex RegExp { get; private set; }

        [MemberMap("_cons", MapModifier.Static, MapType.Constructor)]
        public KRegex(KString pattern)
        {
            Pattern = pattern;
            RegExp = new Regex(Pattern);
        }

        public KRegex(string pattern)
        {
            Pattern = pattern;
            RegExp = new Regex(Pattern);
        }

        [MemberMap("find", MapModifier.Instance, MapType.Method)]
        public KTuple Find(KString text)
        {
            return new KTuple("未实现");
        }

        [MemberMap("test", MapModifier.Instance, MapType.Method)]
        public bool Test(KString text)
        {
            if (text.Length == 0) return false;

            var match = RegExp.Match(text);
            return match != null && match.Length == text.Length;
        }

        [MemberMap("matches", MapModifier.Instance, MapType.Method)]
        public KTuple Matches(KString text)
        {
            List<KString> res = new List<KString>(16);
            foreach (Match match in RegExp.Matches(text))
            {
                res.Add(match.Value);
            }
            return new KTuple(res.ToArray());
        }

        [MemberMap("replace", MapModifier.Instance, MapType.Method)]
        public KString Replace(KString text, KString pattern)
        {
            return RegExp.Replace(text, pattern);
        }

        //[MemberMap("_str", MapModifier.Instance, MapType.Method)]
        public override KString ToStr()
        {
            return Pattern;
        }
    }
}
