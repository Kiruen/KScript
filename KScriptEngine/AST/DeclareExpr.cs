using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class DeclareExpr : ASTList
    {
        public bool IsStatic { get; set; }
        public Modifier Modifier { get; set; }

        public DeclareExpr(List<ASTree> list) : base(list)
        {
            if (list[0] is Modifier)
            {
                var temp = list[0];
                list.RemoveAt(0);
                Modifier = temp as Modifier;
                IsStatic = Modifier.Name == "@";
            }
        }

        public ASTree Declaration
        {
            get { return this[0]; }
        }

        public override object Evaluate(Environment env)
        {
            var temp = new HashSet<string>();
            string varName, prefix = Modifier == null ? "" : Modifier.Name;
            foreach (var ast in this)
            {
                varName = prefix + (ast[0] as ASTLeaf).Text;
                if (temp.Contains(varName))
                    throw new KException("Reduplicated variable declaration!", LineNo);
                else
                {
                    temp.Add(varName);
                    object iniVal = null;
                    if (ast.ChildrenCount > 1)
                    {
                        //赋值语句中的变量名并不会添上@符号,所以这里计算的值没有赋给@v,而是赋给v
                        iniVal = ast[1].Evaluate(env);
                    }
                    env.PutInside(varName, iniVal);
                }
            }
            return null;
        }

        public override string ToString()
        {
            return "var " + Declaration.ToString();
        }
    }
}
