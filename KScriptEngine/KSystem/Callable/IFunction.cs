using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.Callable
{
    public interface IFunction
    {
        IFunction this[int index] { get; set; }
        //接口部分不声明setter,但实现中是可以有的
        string Name { get; }
        int ParamsLength { get; }
        object Invoke(Environment callerEnv, Arguments argList);
    }
}
