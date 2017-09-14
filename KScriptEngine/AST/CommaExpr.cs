using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class CommaExpr : ASTList
    {
        public CommaExpr(List<ASTree> list) : base(list)
        {
            //if (list[0] is BinaryExpr)
            //    list[0] = new ASTList(new List<ASTree>() { list[0] });
        }

        public override object Evaluate(Environment env)
        {
            object res = null;
            foreach (var ast in children)
                res = ast.Evaluate(env);
            return res;
        }

        public override string ToString()
        {
            return children.Select(x => x.ToString())
                           .Aggregate((x, y) => x + y);
        }
    }
}
