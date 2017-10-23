using KScript.AST;
using KScript.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KScript.Callable
{
    //巧用组合模式！只改了三个地方,完美实现了函数重载！
    public class Function : IFunction, ICloneable
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
        //记录函数对象维护的静态环境(内含与单函数实例对应的静态变量)
        protected Environment staticEnv;

        /// <summary>
        /// 根据参数表长度获取具有对应签名的函数实例
        /// </summary>
        /// <param name="i">参数表的长度</param>
        /// <returns></returns>
        public virtual IFunction this[int i]
        {
            get { return this; }
            set { }
        }

        //用于创建OLFunc实例(提供无参的构造函数,于是可以直接实例化OLFunc)
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
            //用于缓存静态值,介于函数零时环境和函数调用环境之间
            staticEnv = new NestedEnv(outerEnv);
        }

        public object GetStaticVar(string varName)
        {
            return staticEnv.Get(varName);
        }

        public void SetStaticVar(string varName, object val)
        {
            staticEnv.PutInside(varName, val);
        }

        public bool HasStaticVar(string varName)
        {
            return staticEnv.Contains(varName);
        }

        /// <summary>
        /// 创建函数实例的临时作用域
        /// </summary>
        /// <returns></returns>
        public Environment CreateNewEnv()
        {
            return new NestedEnv(staticEnv); //outerEnv
        }

        /// <summary>
        /// 对函数进行柯里化(或部分施用),并返回一个新的实例
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public Function curry(IEnumerable<object> args)
        {
            var paramList = new List<ASTree>(4);
            var body = (BlockStmnt)Body.Clone();
            var enumerator = args.GetEnumerator();
            int index = 0;

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

        public object Invoke(Environment callerEnv, Arguments args)
        {
            ParameterList paramsList = Parameters;
            //创建临时的闭包环境
            Environment newEnv = CreateNewEnv();
            //遍历实参(每个参数都可能是一个表达式)列表并计算
            int index = 0;
            paramsList.AssertIsLenMatch(args.ChildrenCount, args.LineNo);
            paramsList.IniVarParams(newEnv/*callerEnv*/);
            foreach (ASTree ast in args)
            {
                //用实参表计算结果,为形参表赋值,放进函数的作用域中
                //函数实例可以使用这个新的作用域内的刚刚计算好的实参变量
                var arg = ast.Evaluate(callerEnv);
                paramsList.Evaluate(newEnv, index++, arg);
            }
            //添加隐含对象
            //newEnv.PutInside("args", )
            //进入调用堆栈
            Debugger.PushFunc(this);    //$"{Name} {Parameters}"
            //执行方法体
            object result = Body.Evaluate(newEnv);
            //从调用堆栈中移除
            Debugger.PopFunc();
            //获取临时作用域返回的值并传递到上层
            return result is InstToken ? ((InstToken)result).Arg : result;
        }

        /// <summary>
        /// 动态调用指定函数实例
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public virtual object Invoke(Environment callerEnv, params object[] args)
        {
            var argList = args == null ? new Arguments(new List<ASTree>())
                    : new Arguments(args.Select(arg => new ASTValue(arg))
                                        .Cast<ASTree>().ToList());
            return Invoke(callerEnv, argList);
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
            set { functions[i] = (Function)value; }
        }

        public OLFunction(params Function[] funcs)
        {
            funcs.Select(func => Add(func))
                 .ToArray();
        }

        public override object Invoke(Environment callerEnv, params object[] args)
        {
            //重新动态调用设计,去除Arguments.Call的无谓包装
            //为Invoke方法提供方便的类型指定
            return Arguments.Call(this, callerEnv, args);
        }

        public IEnumerator<Function> GetEnumerator()
        {
            foreach (var func in functions)
                if (func != null)
                    yield return func;
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
        }

        public Function curry(double paramLen, IEnumerable<object> args)
        {
            Function func = this[(int)paramLen] as Function;
            return func.curry(args);
        }
    }
}