using KScript.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KScript
{
    public abstract class Token
    {
        public static readonly Token EOF = new SpecialToken("EOF", null);
        public static readonly string EOL = "\\n";//";";
	    public int LineNo { set; get; }
        public virtual double Number { get { throw new KException("not a number token.", Debugger.CurrLineNo); } }
        public virtual string Text { get; }

        //抽象单词类
        protected Token(int lineNo)
        {
            LineNo = lineNo;
        }

        public virtual bool IsIdentifier() { return false; }
        public virtual bool IsNumber() { return false; }
        public virtual bool IsString() { return false; }
    }

    /// <summary>
    /// 整型数字单词
    /// </summary>
    class NumToken : Token
    {
        private double numValue;
        public override string Text { get { return numValue.ToString(); } }
        public override double Number { get { return numValue; } }

        private static Dictionary<char, int> radixMap
            = new Dictionary<char, int>()
            {
                {'X', 16 }, {'O', 8 }, {'B', 2 },
                {'x', 16 }, {'o', 8 }, {'b', 2 },
            };
        public NumToken(int lineNo, object value, bool isDec = true) : base(lineNo)
        {
            var val = value.ToString();
            if (isDec)
                numValue = Convert.ToDouble(val);
            else
            {
                int radix = radixMap[val[1]];
                numValue = Convert.ToInt32(val.Substring(2), radix);
            }     
        }

        public override bool IsNumber() { return true; }
    }

    /// <summary>
    /// 标识符单词
    /// </summary>
    class IdToken : Token
    {
        private string name;
        public override string Text { get { return name; } }

        public IdToken(int lineNo, string id)
            : base(lineNo)
        {
            name = id;
        }
        public override bool IsIdentifier() { return true; }
    }

    /// <summary>
    /// 字符串常量单词
    /// </summary>
    class StrToken : Token
    {
        private string literalValue;
        public override string Text { get { return literalValue; } }

        public StrToken(int lineNo, string str)
            : base(lineNo)    
        {
            literalValue = str;
        }
        public override bool IsString() { return true; }
    }

    /// <summary>
    /// 运算符单词
    /// </summary>
    class SymToken : Token
    {
        private string symValue;
        public override string Text { get { return symValue; } }

        public SymToken(int lineNo, string str)
            : base(lineNo)
        {
            symValue = str;
        }
    }

    /// <summary>
    /// 运算符单词
    /// </summary>
    class BoolToken : Token
    {
        private string boolValue;
        public override string Text { get { return boolValue; } }

        public BoolToken(int lineNo, string str)
            : base(lineNo)
        {
            boolValue = str;
        }
    }

    /// <summary>
    /// 特殊单词
    /// </summary>
    public class SpecialToken : Token
    {
        private string instruction;
        public override string Text { get { return instruction; } }
        public object Arg { get; private set; }

        public SpecialToken(string ins, object arg)
            : base(-1)
        {
            instruction = ins;
            Arg = arg;
        }

        public override string ToString()
        {
            return Arg.ToString();
        }
    }
}
