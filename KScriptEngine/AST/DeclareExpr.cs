using KScript.Callable;
using KScript.Runtime;
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
            string varName, realName, 
                    prefix = Modifier == null ? "" : Modifier.Name;
            Function curFunc = Debugger.CurrentFunc as Function;
            bool isFuncStaticVar = curFunc != null && IsStatic;
            foreach (var ast in this)
            {
                realName = (ast[0] as ASTLeaf).Text;
                varName = prefix + realName;
                //对于复杂的逻辑,要善用布尔代数！
                if (temp.Contains(varName) || env.Contains(realName) ||
                    curFunc != null && !IsStatic && curFunc.HasStaticVar(realName))
                    throw new KException("Reduplicated variable declaration!", LineNo);
                else
                {
                    temp.Add(varName);
                    object iniVal = null;
                    //如果是函数静态变量初始化表达式
                    if (isFuncStaticVar)
                    {
                        if (curFunc.HasStaticVar(realName))
                            break;
                        else
                        {
                            if (ast.ChildrenCount > 1)
                                iniVal = ast[1].Evaluate(env);
                            curFunc.SetStaticVar(realName, iniVal);
                            continue;
                        }
                    }
                    //如果是普通初始化表达式,并且带初始值(后面带"=val")
                    else if (ast.ChildrenCount > 1)
                    {
                        //赋值语句中的变量名并不会添上@符号,
                        //所以这里计算的值不会赋给 @var,而是赋给var
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
