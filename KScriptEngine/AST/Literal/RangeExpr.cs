using KScript.Runtime;
using KScript.KSystem.BuiltIn;
using System.Collections.Generic;

namespace KScript.AST
{
    public class RangeExpr : ASTList
    {
        public string LeftSym
        {
            get { return (this[0] as ASTLeaf).Text; }
        }

        public ASTree LeftBound
        {
            get { return this[1].IsEmpty ? null : this[1]; }
        }

        public ASTree RightBound
        {
            get { return this[2].IsEmpty ? null : this[2]; }
        }

        public string RightSym
        {
            get { return (this[3] as ASTLeaf).Text; }
        }
        public RangeExpr(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env)
        {
            double? left = (double?)LeftBound?.Evaluate(env),
                  right = (double?)RightBound?.Evaluate(env);
            return new KRange(LeftSym, left == null ? double.MinValue : left.Value,
                          right == null ? double.MaxValue : right.Value, RightSym);
        }
    }
}
