using KScript.Execution;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ListLiteral : ASTList
    {
        public string Type { get; private set; }

        public int Size
        {
            get { return ChildrenCount; }
        }

        public ListLiteral(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env)
        {
            //List<object> list = new List<object>(8);
            //foreach (ASTree ast in children)
            //    list.Add(ast.Evaluate(env));
            //return list;
            KList list = new KList(8);
            foreach (ASTree ast in children)
                list.Add(ast.Evaluate(env));
            return list;
        }
    }
}
