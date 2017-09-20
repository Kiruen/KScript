using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class MallocStmnt : ASTList
    {
        public MallocStmnt(List<ASTree> list) : base(list) { }

        //TODO:写成函数式的,支持声明任意维数组
        public override object Evaluate(Environment env)
        {
            int index1 = Convert.ToInt32(children[0].Evaluate(env));
            var array = new object[index1];
            if (ChildrenCount == 2)
                array = array.Select(a => new object[Convert.ToInt32(children[1].Evaluate(env))])
                             .ToArray();
            return array;
        }
    }
}
