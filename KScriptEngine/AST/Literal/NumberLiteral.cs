using KScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class NumberLiteral : ASTLeaf
    {
        public NumberLiteral(Token token) : base(token) { }
        public double Value { get { return Token.Number; } }
        public override object Evaluate(Environment env)
        {
            return Value;
        }
    }
}
