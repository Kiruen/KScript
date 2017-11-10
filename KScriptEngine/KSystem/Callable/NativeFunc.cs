using KScript.AST;
using KScript.Runtime;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KScript.Callable
{
    //C#原生函数代理
    public class NativeFunc : NativeMember, IFunction
    {
        public virtual bool IsOLFuncSet => false;
        public int ParamsLength { get; private set; }

        /// <summary>
        /// 指眀函数是否为延迟构造的函数(用于某个对象访问非确定的成员)
        /// </summary>
        public bool IsDeferred { get; private set; }
        /// <summary>
        /// 指明函数的参数表是否为(含有)可变参数表
        /// </summary>
        public bool IsVarParams { get; private set; }

        public MethodBase Method { get; private set; }

        public virtual IFunction this[int index]
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

        public NativeFunc(string name, MethodBase method, bool isVarParams, object invoker = null)
            : base(name, invoker)
        {
            Method = method;
            ParamsLength = method.GetParameters().Length;
            IsVarParams = isVarParams;
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
            ParamsLength = 1024;
            IsStatic = isStatic;
        }

        public object Invoke(Environment callerEnv, Arguments argList)
        {
            if (argList.Length != ParamsLength && !IsVarParams && !IsDeferred)
                throw new KException("bad number of args in invokation", Debugger.CurrLineNo);
            object[] args = argList.Select(ast => ast.Evaluate(callerEnv)).ToArray();
            //进入调用堆栈
            Debugger.PushFunc(this);
            //执行方法体
            object result = result = Invoke(callerEnv, args);
            //从调用堆栈中移除
            Debugger.PopFunc();
            return result;
        }

        /// <summary>
        /// 调用原生函数
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="srcArgs"></param>
        /// <returns></returns>
        public virtual object Invoke(Environment callerEnv, params object[] srcArgs)
        {
            try
            {
                //动态构造Method(用于临时的函数调用)
                //即使args长度为0也是没问题的
                //注意！！！调用者可能为空(因为静态方法不需要指定调用者)
                if (Method == null)
                {
                    srcArgs = srcArgs.Where(arg => arg != null)
                               //.Select(arg => arg is double ? Convert.ToInt32(arg) : arg)
                               .ToArray();
                    var type = IsStatic ? invoker as Type : invoker.GetType();
                    Method = type.GetMethod(Name, srcArgs.Select(arg => arg.GetType())
                                                      .ToArray());
                }
                //调用Method
                object result = null;
                //将参数转换为原生函数接受的形式(包括可变长参数表的转换)
                var args = ConvertArgs(srcArgs);
                if (Method is ConstructorInfo)
                    result = (Method as ConstructorInfo).Invoke(args);
                else
                    result = Method.Invoke(invoker, args);

                return ConvertRet(result);
            }

            catch (Exception exc)
            {
                throw new KException($"bad native function call: {Name}" +
                            $"\r\nSourceError:\r\n{exc.Message}" +
                            $"\r\n{exc.InnerException?.Message}",
                            Debugger.CurrLineNo);
            }
        }

        private object[] ConvertArgs(object[] args)
        {
            var paramList = Method.GetParameters();
            int plen = paramList.Length;
            var temp = new object[plen];
            
            if (IsVarParams)
            {
                temp[plen - 1] = args.Skip(plen - 1).ToArray();
                plen--;
            }
            for (int i = 0; i < plen; i++)
            {
                Type ptype = paramList[i].ParameterType,
                                atype = args[i]?.GetType();
                if (atype != ptype && !ptype.IsInstanceOfType(args[i]))
                    temp[i] = Convert.ChangeType(args[i], ptype);
                else
                    temp[i] = args[i];
            }
            return temp;
        }

        private object ConvertRet(object result)
        {
            if (result == null) return null;
            //返回类型转换
            else if (result.GetType().IsValueType)
                return Convert.ToDouble(result);
            else if (result is string)
                return (KString)(result as string);
            return result;
        }

        public override string ToString()
        { return "<native: " + GetHashCode() + ">"; }
    }

    public class OLNativeFunc : NativeFunc
    {
        public static readonly int POS_OF_VARLEN = 10;
        private NativeFunc[] functions = new NativeFunc[11];
        public virtual bool IsOLFuncSet => true;

        public override IFunction this[int i]
        {
            get
            {
                if (i > POS_OF_VARLEN)
                    i = POS_OF_VARLEN;
                //该位置函数没有重载定义,则转到含变长参数表的函数(可能为null)
                if (functions[i] == null)
                    return functions[POS_OF_VARLEN];
                return functions[i];
            }
            set { functions[i] = (NativeFunc)value; }
        }

        public OLNativeFunc(params NativeFunc[] funcs)
        {
            funcs.Select(func => Add(func))
                 .ToArray();
        }

        public override object Invoke(Environment callerEnv, params object[] args)
        {
            return Arguments.Call(this, callerEnv, args);
            /*this[args.Length].Invoke(callerEnv, args);*/
        }

        public bool Add(NativeFunc func)
        {
            int len = func.ParamsLength;
            if (func.IsVarParams)
            {
                functions[POS_OF_VARLEN] = func;
                return true;
            }
            else if (len < POS_OF_VARLEN)
            {
                functions[len] = func;
                return true;
            }
            return false;
        }
    }
}
