using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class Pair : ASTList
    {
        public Pair(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env)
        {
            return Tuple.Create(children[0].Evaluate(env),
                                children[1].Evaluate(env));
        }
    }
}
