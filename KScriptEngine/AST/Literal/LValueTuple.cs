using KScript.Runtime;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class LValueTuple : ASTList
    {
        //string[] VarNames;
        protected InitalList InitList { get; set; }
        public LValueTuple(List<ASTree> list) : base(list)
        {
            InitList = list[0] as InitalList;
            //VarNames = children.OfType<ASTLeaf>()
            //                   .Select(id => id.Text)
            //                   .ToArray();
        }

        //在系统自动求值时会返回一个右值的元组
        public override object Evaluate(Environment env)
        {
            return new KTuple(InitList.Select(ast => ast.Evaluate(env)).ToArray());
        }

        public void Assign(object obj, Environment env)
        {
            if (obj is KTuple tuple)
            {
                int i = 0;
                foreach (var ast in InitList)
                    BinaryExpr.Assign(ast, tuple[i++], env);
            }
            else
            {
                BinaryExpr.Assign(InitList[0], obj, env);
            }
        }
    }
}
