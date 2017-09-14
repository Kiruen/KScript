using KScript.Execution;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class TupleLiteral : ASTList
    {
        public TupleLiteral(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env)
        {
            return new KTuple(children.Select(ast => ast.Evaluate(env)).ToArray());
        }
    }
}
