using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KScript
{
    /// <summary>
    /// 词法分析器
    /// </summary>
    public class Lexer
    {
        private static Regex regex = new Regex(
        "\\s*((//.*)" +
        "|([0O][xobXOB][\\da-fA-F]+)" +
        "|(\\.\\d+|[0-9]+(\\.[0-9]+)?)" +
        "|((\"|')(\\\\\\7|\\\\\\\\|\\\\n|\\\\r|\\\\t|[^\\7])*?\\7)" +
        "|(true|false)" +
        "|(in\\b|is\\b|not\\b|or\\b|and\\b)" +
        "|([A-Z_a-z\u4e00-\u9fa5][A-Z_a-z0-9\u4e00-\u9fa5]*)" +
        "|(\\.\\.\\.|\\*\\*=|>>=|<<=|!=|==|<=|>=|<<|>>|\\*\\*|&&|\\|\\||\\+=|-=|\\*=|/=|\\%=|&=|\\|=|\\^=|=>|::|\\p{P}|\\p{S})" + 
        ")?"
        , RegexOptions.Compiled);

        //static string pattern =
        //   @"\s*(
        //     (//.*)
        //    |(0[xob][\da-fA-F]+)
        //    |(\.\d+|[0-9]+(\.[0-9]+)?)
        //    |((""|')(\\\7|\\\\|\\n|\\r|\\t|[^\7])*?\7)
        //    |(true|false)
        //    |(in\b|is\b)
        //    |([A-Z_a-z][A-Z_a-z0-9]*)
        //    |(\*\*=|>>=|<<=|!=|==|<=|>=|<<|>>|\*\*|&&|\|\||\+=|-=|\*=|/=|\%=|&=|\|=|\^=|=>|::|\p{P}|\p{S})
        //   )?";

        //private static Regex regex = new Regex(pattern, RegexOptions.Compiled);
        //↑将包含所有单字符运算符哦!还有注意x=的"+,*,%"也是正则表达式的运算符,所以要加'\'
        //    Regex regex = new Regex("\\s*((//.*)|([0-9]+(\\.[0-9]+)?)|(\"(\\\\\"|\\\\\\\\|\\\\n|[^\"])*\")"
        //+ "|[A-Z_a-z][A-Z_a-z0-9]*|==|<=|>=|&&|\\|\\||\\p{P}|\\p{S})?", RegexOptions.Compiled);

        public bool HasMore { set; private get; }
        public int TokenCount { get { return tokens.Count; } }
        public string Script { get; private set; }

        private StringReader reader;
        private Queue<Token> tokens = new Queue<Token>(1024);
        //单词流备份
        private List<Token> duplication = new List<Token>(1024);

        public Lexer(string code, bool preProc = true)
        {
            //决定是否进行预处理
            if(preProc)
                Script = PreProcessor.GetPreProcessing(code);
            else
                Script = code;

            HasMore = true;
            reader = new StringReader(Script);
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }
        
        /// <summary>
        /// 取出最前面的Token
        /// </summary>
        /// <returns></returns>
        public Token Read()
        {
            if (FillQueue(0))
            {
                //Token token = tokens[0];
                //tokens.RemoveAt(0);
                //return token;
                return tokens.Dequeue();
            }
            else
                return Token.EOF;
        }

        ///// <summary>
        ///// 向前侦查offset处的单词
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <returns></returns>
        //public Token Spy(int offset)
        //{
        //    return tokens.ElementAt(offset);
        //}

        public Token Peek(int i)
        {
            if (FillQueue(i))
                return tokens.Peek(); // tokens[i]
            else
                return Token.EOF;
        }

        /// <summary>
        /// 为Token队列补充新Token
        /// </summary>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        private bool FillQueue(int endIndex)
        {
            while (endIndex >= tokens.Count)
            {
                if (HasMore)
                    ReadLine();
                else
                    return false;
            }
            return true;
        }

        public void ReadAll()
        {
            if(duplication.Count == 0)
            {
                while (HasMore)
                    ReadLine();
                foreach (var token in tokens)
                    duplication.Add(token);
            }
            else
            {
                foreach (var token in duplication)
                    tokens.Enqueue(token);
            }
        }

        int currentLineNo = 0;
        /// <summary>
        /// 由字符流中获取一行代码(可能读取一段多行注释)
        /// </summary>
        protected void ReadLine()
        {
            string line = "";
            if ((line = reader.ReadLine()) != null)
            {
                currentLineNo++;
                //碰到多行注释(无法通过正则表达式匹配！因为是按行分析的)
                if(line.TrimStart().StartsWith("/*"))
                {
                    while (!line.TrimEnd().EndsWith("*/"))
                    {
                        line = reader.ReadLine();
                        currentLineNo++;
                    }
                    ReadLine();
                    return;
                }
                MatchCollection matcher = regex.Matches(line);
                foreach (Match match in matcher)
                    AddToken(currentLineNo, match);
                //tokens.Enqueue(new SymToken(lineNo, Token.EOL));
            }
            //读到的单词是空的,说明词法分析已结束
            if (line == null)
            {
                HasMore = false;
                reader.Close();
            }
        }

        /// <summary>
        /// 由捕获到的Match创建并添加Token
        /// </summary>
        /// <param name="lineNo">Token所在代码行的行号</param>
        /// <param name="match">捕获到的匹配项</param>
        protected void AddToken(int lineNo, Match match)
        {
            string val = match.Groups[1].Value;
            if (val != string.Empty)
            {
                if (match.Groups[2].Value == string.Empty) //不是注释
                {
                    Token token = null;
                    if (match.Groups[3].Value != string.Empty)
                        token = new NumToken(lineNo, val, false);
                    else if (match.Groups[4].Value != string.Empty)
                        token = new NumToken(lineNo, val);
                    else if (match.Groups[6].Value != string.Empty)
                        token = new StrToken(lineNo, ToStringLiteral(val));
                    else if(match.Groups[9].Value != string.Empty)
                        token = new NumToken(lineNo, val == "true" ? 1 : 0);
                    else if (match.Groups[11].Value != string.Empty)
                        token = new IdToken(lineNo, val);
                    //9、11号分组,分别为单词运算符、符号运算符
                    else
                        token = new SymToken(lineNo, val);
                    //tokens.Add(token);
                    tokens.Enqueue(token);
                }
            }
        }

        protected string ToStringLiteral(string str)
        {
            StringBuilder strBd = new StringBuilder();
            int length = str.Length - 1;
            for (int i = 1; i < length; i++)
            {
                char ch = str[i];
                if (ch == '\\' && i + 1 < length)
                {
                    //预判
                    char ch2 = str[i + 1];
                    if (ch2 == '"' || ch2 == '\'' || ch2 == '\\')
                        ch = str[++i];
                    else if (ch2 == 'n')
                    {
                        ++i; ch = '\n';
                    }
                    else if (ch2 == 'r')
                    {
                        ++i; ch = '\r';
                    }
                    else if (ch2 == 't')
                    {
                        ++i; ch = '\t';
                    }
                }
                strBd.Append(ch);
            }
            return strBd.ToString();
        }
    }
}
