using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class NegativeExpr : ASTList
    {
        public NegativeExpr(List<ASTree> c) : base(c) { }
        public ASTree Operand() { return this[0]; }

        public override object Evaluate(Environment ev)
        {
            return -(double)Operand().Evaluate(ev);
        }

        public override string ToString()
        {
            return "-" + Operand();
        }
    }
}
