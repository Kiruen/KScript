using KScript.Callable;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class DefStmnt : ASTList
    {
        public DefStmnt(List<ASTree> list) : base(list)
        {
            if (list[0] is Modifier)
            {
                var temp = list[0];
                list.RemoveAt(0);
                Modifier = temp as Modifier;
                IsStatic = Modifier.Name == "@";
            }
        }

        public string Name
        { get { return ((ASTLeaf)children[0]).Text; }  }

        public ParameterList Parameters
        { get { return (ParameterList)children[1]; } }

        public BlockStmnt Body
        { get { return (BlockStmnt)children[2]; } }

        //-----------------------------//
        public Modifier Modifier { get; private set; }

        public bool IsStatic { get; private set; }
        //-----------------------------//

        public override object Evaluate(Environment env)
        {
            var totalName = Modifier?.Name + Name;
            var newFunc = new Function(Name, Parameters, Body, env);
            if (env.Contains(totalName))
            {
                var olFunc = env.Get(totalName) as OLFunction;
                if (olFunc != null)
                    olFunc.Add(newFunc);
                //存在原生函数,则不覆盖,而是添加到主命名空间中
                else
                    (env.Get("main") as KNameSpace)
                        ?.AddMember(totalName, new OLFunction(newFunc));
            }
            else
                env.PutInside(totalName, new OLFunction(newFunc));
            return totalName;
        }

        public override string ToString()
        {
            return string.Format("<def: {0} {1} {2}>", Name, Parameters, Body);
        }
    }
}
