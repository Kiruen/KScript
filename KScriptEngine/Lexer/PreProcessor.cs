using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KScript
{
    public static class PreProcessor
    {
        //^表示匹配的是整个字符串的开头(而不是一行)
        //$表示整个字符串的末尾
        public static Regex reg = new Regex(@"#(?<inst>replace|rep)(?<args>( [\w\S]+)+)", RegexOptions.Compiled);

        public static string GetPreProcessing(string text)
        {
            MatchCollection matches = reg.Matches(text);
            if (matches.Count == 0)
                return text;
            var sb = new StringBuilder(text);
            int start = matches[0].Index;
            var theLast = matches[matches.Count - 1];
            sb.Remove(start, theLast.Index + theLast.Length - start);
            foreach (Match match in matches)
            {
                string instName = match.Groups["inst"].Value;
                string[] args = match.Groups["args"].Value.Trim().Split(' ');
                switch (instName)
                {
                    case "replace": case "rep":
                        { sb.Replace(args[0], args[1]); break; }
                }
            }
            return sb.ToString();
        }
    }
}
