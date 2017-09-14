using KScript.AST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KScript.Callable
{
    //巧用组合模式！只改了三个地方,完美实现了函数重载！
    public class Function : ICloneable
    {
        public string Name { get; set; }
        public ParameterList Parameters { get; private set; }
        public int ParamsLength
        {
            get { return Parameters.Length; }
        }
        public BlockStmnt Body { get; private set; }
        //函数的闭包环境(记录外部信息的环境)
        protected Environment outerEnv;

        /// <summary>
        /// 根据参数表长度获取具有对应签名的函数实例
        /// </summary>
        /// <param name="i">参数表的长度</param>
        /// <returns></returns>
        public virtual Function this[int i]
        {
            get { return this; }
            protected set { }
        }

        //用于创建OLFunc实例(提供无参构造函数)
        protected Function() { }

        /// <summary>
        /// 创造一个函数实例
        /// </summary>
        /// <param name="parameters">形参列表的语法树</param>
        /// <param name="body">函数体的语法树</param>
        /// <param name="env">函数被定义的作用域</param>
        public Function(string name, ParameterList parameters, BlockStmnt body, Environment env)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
            outerEnv = env;
        }

        /// <summary>
        /// 创建函数实例的临时作用域
        /// </summary>
        /// <returns></returns>
        public Environment CreateNewEnv() { return new NestedEnv(outerEnv); }

        /// <summary>
        /// 对函数进行柯里化(或部分施用),并返回一个新的实例
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public Function curry(IEnumerable<object> args)
        {
            var paramList = new List<ASTree>(4);
            var body = (BlockStmnt)Body.Clone();

            int index = 0;
            var enumerator = args.GetEnumerator();

            foreach (var child in Parameters)
            {
                if (!enumerator.MoveNext())
                    paramList.Add(child);
                else
                    //为指定个数的参数赋初值
                    body.InsertCode(index, Parameters.ParamName(index) +
                                            "=" + enumerator.Current);
                index++;
            }
            return new Function(Name, new ParameterList(paramList), body, outerEnv);
        }

        /// <summary>
        /// 动态调用指定函数实例
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public object Invoke(Environment callerEnv, params object[] args)
        {
            return Invoke(this, callerEnv, args);
        }

        /// <summary>
        /// 动态调用指定函数实例
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static object Invoke(object func, Environment callerEnv, params object[] args)
        {
            return Arguments.Invoke(func, callerEnv, args);
        }

        /// <summary>
        /// 向重载函数列表中增加新函数
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public virtual bool Add(Function func)
        {
            return false;
        }

        public override string ToString()
        {
            return "<func: " + GetHashCode() + ">";
        }

        /// <summary>
        /// 拷贝函数定义
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            //这里的参数表定义可以共用对象
            return new Function(Name, Parameters, Body.Clone() as BlockStmnt, outerEnv);
        }
    }

    //重载函数封装类
    public class OLFunction : Function, IEnumerable<Function>
    {
        public static readonly int POS_OF_VARLEN = 10;
        private Function[] functions = new Function[11];
        //TODO:实现变长参数表
        public override Function this[int i]
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
            protected set { functions[i] = value; }
        }

        public OLFunction(params Function[] funcs)
        {
            funcs.Select(func => Add(func))
                 .ToArray();
        }

        public IEnumerator<Function> GetEnumerator()
        {
            foreach (var func in functions)
                if (func != null) yield return func;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool Add(Function func)
        {
            int len = func.ParamsLength;
            if (func.Parameters.IsVarParams)
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
            //if(functions[len] != null)
            //{
            //    if (!functions[len].Parameters.IsVarLength)
            //    {
            //        functions[POS_OF_VARLEN] = func;
            //        return true;
            //    }
            //    else if (!func.Parameters.IsVarLength)
            //    {
            //        functions[POS_OF_VARLEN] = functions[len];
            //        functions[len] = func;
            //        return true;
            //    }
            //}
            ////其他情况(都是或都不是含有变长参数表的函数,则覆盖)
            //else if (len <= POS_OF_VARLEN)
            //{
            //    functions[len] = func;
            //    return true;
            //}
            //return false;
        }

        public Function curry(double paramLen, IEnumerable<object> args)
        {
            var func = this[(int)paramLen];
            return func.curry(args);
        }
    }
}