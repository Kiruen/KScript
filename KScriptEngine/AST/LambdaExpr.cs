using KScript.Callable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class LambdaExpr : ASTList
    {
        public LambdaExpr(List<ASTree> list) : base(list)
        {
            if (!(children[1] is BlockStmnt))
            {
                children[1] = new BlockStmnt(new List<ASTree> { children[1] });
            }
        }

        public ParameterList Parameters
        { get { return (ParameterList)children[0]; } }

        public BlockStmnt Body
        { get { return (BlockStmnt)children[1]; } }

        public override object Evaluate(Environment env)
        {
            //返回函数实例的副本(一个闭包)
            //若定义在函数体内,则当最终返回函数的body.eval时
            //返回这个副本,原函数的环境被保存下来,而不是在Arguments.eval执行后被GC回收
            //此副本返回到调用它的环境里,里面的变量也同时被保存着
            //所以多次调用同一个变量引用的闭包可能会改变原环境内部的变量值
            return new Function("Lambda", Parameters, Body, env);
        }

        public override string ToString()
        {
            return "<func " + Parameters + " " + Body + ">";
        }
    }
}
