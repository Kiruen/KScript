using KScript.Runtime;
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
        protected InitalList InitList { get; set; }
        public int Size
        {
            get { return ChildrenCount; }
        }

        public ListLiteral(List<ASTree> list) : base(list)
        {
            InitList = list[0] as InitalList;
        }

        public override object Evaluate(Environment env)
        {
            KList list = new KList(8);
            foreach (ASTree ast in InitList)
            {
                list.Add(ast.Evaluate(env));
            }
            return list;
        }
    }
}
