using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class MonadExpr : ASTList
    {
        public IEnumerable<string> Operators { get; set; } //get { return this[0]; }
        public ASTree Operand { get { return this[ChildrenCount - 1]; } }

        public MonadExpr(List<ASTree> c) : base(c)
        {
            Operators = this.Take(ChildrenCount - 1)
                            .Cast<ASTLeaf>()
                            .Select(leaf => leaf.Text)
                            .Reverse();     //由内向外运算
        }

        public override object Evaluate(Environment ev)
        {
            object res = Operand.Evaluate(ev);
            foreach (var op in Operators)
            {
                if (res is double)
                    ComputeNum(op, ref res);
                else
                    ComputeObj(op, ref res);
                //错误的设计！如何改进？
                //switch (op)
                //{
                //    case "-": res = -res; break;
                //    case "!": res = BOOL[res == 0]; break;
                //    case "~": res = ~(int)res; break;
                //    case "&": res = res.GetHashCode(); break;
                //    case "@": res = Math.Abs(res); break;
                //}
            }
            return res;
        }

        public void ComputeNum(string op, ref object val)
        {
            double dbleVal = Convert.ToDouble(val);
            switch (op)
            {
                case "-": val = -dbleVal; break;
                case "!": case "not": val = BOOL[dbleVal == 0]; break;
                case "~": val = ~(int)dbleVal; break;
                case "&": val = dbleVal.GetHashCode(); break;
                case "@": val = Math.Abs(dbleVal); break;
            }
        }

        public void ComputeObj(string op, ref object val)
        {
            switch (op)
            {
                case "&": val = val.GetHashCode(); break;
            }
        }

        public override string ToString()
        {
            return Operators + " " + Operand;
        }
    }
}
