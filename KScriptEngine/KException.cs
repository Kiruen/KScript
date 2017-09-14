using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KScript.AST;

namespace KScript
{
    public class KException : Exception
    {
        public int Location { get; set; }

        public KException(string msg, int loc) 
            : base("Runtime error: " + msg + "\r\nAt line: " + loc)
        {
            Location = loc;
        }

        public KException(string msg, object arg, int loc) 
            : this(msg + arg, loc)
        { }
    }

    public class ParseException : Exception
    {
        public ParseException(Token t) : this("", t)
        { }
        public ParseException(string msg, Token t)
            : base("syntax error around " + location(t) + ". " + msg)
        { }
        private static string location(Token t)
        {
            if (t == Token.EOF)
                return "the last line";
            else
                return "\"" + t.Text + "\" at line " + t.LineNo;
        }
        public ParseException(IOException e) : base(e.Message)
        { }

        public ParseException(string msg) : base(msg)
        { }
    }
}
