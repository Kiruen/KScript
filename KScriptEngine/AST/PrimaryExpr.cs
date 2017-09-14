using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class PrimaryExpr : ASTList
    {
        public ASTree Operand
        { get { return children[0]; } }

        public PrimaryExpr(List<ASTree> list) : base(list) { }
        public static ASTree Create(List<ASTree> list)
        {
            return list.Count == 1 ? list[0] : new PrimaryExpr(list);
        }

        /// <summary>
        /// 返回nest层的后缀,类似:func(func(1))
        /// </summary>
        /// <param name="nest">从里往外数的层序号</param>
        /// <returns></returns>
        public Postfix Postfix(int nest)
        {
            return (Postfix)children[ChildrenCount - nest - 1];
        }

        public bool HasPostfix(int nest)
        {
            return ChildrenCount - nest - 1 > 0; //??
        }

        public override object Evaluate(Environment env)
        {
            return EvalSubExpr(env, 0);
        }

        public object EvalSubExpr(Environment env, int nest)
        {
            //判断前面是否有"后缀"(意思是说一旦碰到varname就判断为无后缀)
            if (HasPostfix(nest))
            {
                //可能是函数调用的嵌套:Func1(1)(2);
                //注意！！并不是Func(Func(2))这种形式！！！！
                //向内获取函数实例,然后执行这个函数(这个函数有可能还会返回
                //一个函数实例,若此,则将实例返回给右面的后缀)
                //例如Func1(1)(2) => <Func1(1)>(2) => AFunc(2) => 1;
                object target = EvalSubExpr(env, nest + 1);
                    //返回函数调用结果(可能有很多层嵌套哦)
                    //TODO:实现函数的重载(多态性 will be gotten!)
                    //初步思路:使用Lookup; 取函数实例时匹配参数表个数
                return Postfix(nest).Evaluate(env, target);
            }
            else    //没有实参,也就不是函数,就当成普通的基础表达式计算
                    //!!然而在函数调用的语法中,此处返回最里层函数名称
                    //对应的函数实例(来自env中)
                return Operand.Evaluate(env);
        }
    }
}
