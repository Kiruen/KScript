using KScript.Execution;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using KScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KScript.AST
{
    public class StringLiteral : ASTLeaf
    {
        public StringLiteral(Token t) : base(t) { }
        public string Value { get { return base.Text; } }

        public override object Evaluate(Environment env)
        {
            return KString.Instance(Value);
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }
    }

    public class ExprStringLiteral : StringLiteral
    {
        public ExprStringLiteral(Token t) : base(t) { }

        public override object Evaluate(Environment env)
        {
            base.Evaluate(env);
            StringBuilder source = new StringBuilder(Value);
            var matches = Regex.Matches(base.Text, @"\{(?<exp>.+?)\}");
            Lexer lexer;
            foreach(Match match in matches)
            {
                lexer = new Lexer(match.Groups["exp"].Value);
                var ast = Evaluator.Parse(lexer);
                var res = ast.Evaluate(env);
                if (res == null) res = "None";
                source.Replace(match.Value, KUtil.ToString(res));
            }
            return KString.Instance(source);
        }
    }

    public class RegexLiteral : StringLiteral
    {
        public RegexLiteral(Token t) : base(t) { }

        public override object Evaluate(Environment env)
        {
            return new KRegex(Value);
        }
    }
}
