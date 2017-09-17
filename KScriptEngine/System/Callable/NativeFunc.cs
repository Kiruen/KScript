using KScript.AST;
using KScript.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KScript.Callable
{
    //C#原生函数代理
    public class NativeFunc : NativeMember, IFunction
    {
        public int ParamsLength { get; private set; }

        /// <summary>
        /// 指眀函数是否为延迟构造的函数(用于某个对象访问非确定的成员)
        /// </summary>
        public bool IsDeferred { get; private set; }
        public MethodBase Method { get; private set; }

        public IFunction this[int index]
        {
            get
            {
                return this;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected NativeFunc() { }

        public NativeFunc(string name, MethodBase method, object invoker = null)
            : base(name, invoker)
        {
            Method = method;
            ParamsLength = method.GetParameters().Length;
        }

        /// <summary>
        /// 延迟构造一个临时的函数实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="invoker"></param>
        public NativeFunc(string name, object invoker, bool isStatic = false)
            : base(name, invoker)
        {
            IsDeferred = true;
            ParamsLength = 5;
            IsStatic = isStatic;
        }

        public object Invoke(Environment callerEnv, Arguments argList)
        {
            if (argList.Length != ParamsLength && !IsDeferred)
                throw new KException("bad number of args in invokation", Debugger.CurrLineNo);
            object[] _args = new object[ParamsLength];
            int index = 0;
            foreach (ASTree ast in argList)
            {
                //计算实参表中的参数,可能又会跑到primary中
                _args[index++] = ast.Evaluate(callerEnv);
            }
            //进入调用堆栈
            Debugger.PushFunc(Name);
            //执行方法体
            object result = Invoke(_args);
            //从调用堆栈中移除
            Debugger.PopFunc();
            return result;
        }

        /// <summary>
        /// 调用原生函数
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(params object[] args)
        {
            try
            {
                //动态构造Method(用于临时的函数调用)
                //即使args长度为0也是没问题的
                //注意！！！调用者可能为空(因为静态方法不需要指定调用者)
                if (Method == null)
                {
                    args = args.Where(arg => arg != null)
                               //.Select(arg => arg is double ? Convert.ToInt32(arg) : arg)
                               .ToArray();
                    var type = IsStatic ? invoker as Type : invoker.GetType();
                    Method = type.GetMethod(Name, args.Select(arg => arg.GetType())
                                                      .ToArray());
                }
                //调用Method
                object result = null;
                args = ConvertArgs(args).ToArray();
                if (Method is ConstructorInfo)
                    result = (Method as ConstructorInfo).Invoke(args);
                else
                    result = Method.Invoke(invoker, args);
                //返回类型转换
                if (result != null && result.GetType().IsValueType)
                    result = Convert.ToDouble(result);
                return result;
            }
            catch (Exception exc)
            {
                throw new KException("bad native function call: " + Name 
                    + "\r\nSourceError:\r\n" + exc.Message, Debugger.CurrLineNo);
            }
        }

        private IEnumerable<object> ConvertArgs(object[] args)
        {
            var paramList = Method.GetParameters();
            for(int i = 0; i < paramList.Length; i++)
            {
                Type ptype = paramList[i].ParameterType,
                                atype = args[i]?.GetType();
                if (atype != ptype && !ptype.IsInstanceOfType(args[i]))
                    yield return Convert.ChangeType(args[i], ptype);
                else yield return args[i];
            }
        }
        //public object Invoke(ASTree tree, params object[] args)
        //{  }

        public override string ToString()
        { return "<native: " + GetHashCode() + ">"; }
    }

    public class OLNativeFunc
    {
        public OLNativeFunc() { }
    }
}
