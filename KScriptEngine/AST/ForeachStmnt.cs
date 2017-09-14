using KScript.Callable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ForeachStmnt : ASTList
    {
        //用于每次迭代的变量名称
        public string Status { get; private set; }
        //集合(迭代器)对象
        public ASTree Enumrator { get { return this[1] as ASTree; } }
        public ASTree Body { get { return this[2]; } }

        public ForeachStmnt(List<ASTree> list) : base(list)
        {
            Status = (this[0] as ASTLeaf).Text;
        }

        public override object Evaluate(Environment env)
        {
            object result = 0;
            int stackLevel = 0;
            //循环语句私有的作用域(注意与代码块的独立作用域加以区分),保存一些状态变量
            Environment inner = new NestedEnv(env);
            //获取要迭代的集合的迭代器
            var enumObj = Enumrator.Evaluate(env);
            //剥开KObject的包装,获取最原始的数据集合
            while (!(enumObj is IEnumerable<object>))
            {
                if(enumObj is KObject)
                {
                    enumObj = Function.Invoke((enumObj as KObject).Read("iterator"), env);
                }
            }
            var collection = enumObj as IEnumerable<object>;
            var enumrator = collection.GetEnumerator();
            //初始化当前的集合元素(启动状态机),并向环境中置初值
            enumrator.MoveNext();
            inner.PutInside(Status, enumrator.Current);
            while (stackLevel++ < STACK_MAXLEVEL)
            {
                if (result is SpecialToken)
                {
                    SpecialToken token = result as SpecialToken;
                    if (token.Text == "break")
                        return null;
                    else if (token.Text == "continue")
                    {
                        result = null; //清空result,进行新一次循环
                        continue;
                    }
                    else if (token.Text == "return")
                        return token;
                }
                //改变状态
                else
                {
                    result = Body.Evaluate(inner);
                    if (!enumrator.MoveNext()) return null;
                    else inner.Put(Status, enumrator.Current);
                }
            }
            throw new KException("Stack overflow!", LineNo);
        }

        public override string ToString()
        {
            return "<foreach " + Status + " in " + Enumrator
                   + " " + Body + ">";
        }
    }
}
