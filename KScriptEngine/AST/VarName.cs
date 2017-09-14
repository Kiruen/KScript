using KScript.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class VarName : ASTLeaf
    {
        public string Name { get { return base.Text; } }
        public VarName(Token token) : base(token) { }

        //试图获取环境中的变量值
        public override object Evaluate(Environment env)
        {
            object result = env.Get(Name);
            //if (!ev.Contains(Name)) throw new Exception();
            //简化代码(空参数表的原生函数可直接用名称调用)
            //if (result is NativeFunction)
            //{
            //    var method = result as NativeFunction;
            //    if(method.ParamsLength == 0)
            //        return method.Invoke(null, this);
            //}
            return result;
            //catch
            //{
            //    throw new KException("Variable doesn't exists", Location);
            //}
        }
    }
}
