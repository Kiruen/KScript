using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class WhileStmnt : ASTList
    {
        public ASTree Condition { get { return this[0]; } }
        public ASTree Body { get { return this[1]; } }

        public WhileStmnt(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env)
        {
            object result = 0;
            int stackLevel = 0;
            //循环语句私有的作用域,保存一些状态变量
            Environment inner = new NestedEnv(env);
            while (stackLevel++ < STACK_MAXLEVEL)
            {
                object condiRes = Condition.Evaluate(env);
                if (!(condiRes is double))
                    throw new KException("Invalid state variable!", LineNo);
                else if ((double)condiRes == 0)
                    return null;
                else if (result is InstToken)
                {
                    InstToken token = (InstToken)result;
                    if (token.Inst == InstToken.BREAK)
                        return null;
                    else if (token.Inst == InstToken.CONTINUE)
                    {
                        result = null; //清空result,进行新一次循环
                        continue;
                    }
                    else if (token.Inst == InstToken.RETURN)
                        return token;
                }
                else
                    result = Body.Evaluate(inner);
            }
            throw new KException("Stack overflow!", LineNo);
        }

        //子类new 父类引用子类实例时只能访问到父类的方法
        //子类override(与 virtual搭配) 父类引用子类实例时访问子类的方法
        public override string ToString()
        {
            return "<while " + Condition + " " + Body + ">";
        }
    }
}
