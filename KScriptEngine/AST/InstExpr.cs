using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    //系统内建指令
    public class InstExpr : ASTList
    {
        public string InstName { get; private set; }
        public ASTree MainArg { get { return this[1]; } }
        public ASTree DeputyArg { get; private set; }

        public InstExpr(List<ASTree> list) : base(list)
        {
            InstName = (list[0] as ASTLeaf).Text;
            //添加指令(针对using)的附属参数表
            if(list.Count >= 3)
                DeputyArg = ASTList.MakeList(list[2]);
        }

        public override object Evaluate(Environment env)
        {
            switch (InstName)
            {
                case "continue": case "break":
                    return new SpecialToken(InstName, null);
                case "return": return new SpecialToken(InstName, this[1].Evaluate(env));
                case "new": return InstFactory.Malloc(MainArg, env);
                case "using": return InstFactory.Using(MainArg, DeputyArg, env);
                case "del": return InstFactory.Delete(MainArg, env);
                case "throw": return InstFactory.Throw(MainArg, env, LineNo);
                case "assert": return InstFactory.Assert(MainArg, env, LineNo);
                default: throw new KException("Invalid instruction!", LineNo);
            }
        }
    }

    //指令工厂,提供执行各种指令的方法
    public static class InstFactory
    {
        //ast为去除指令后的抽象语法树,通常为指令所需要的参数

        //空间申请指令
        //TODO:统一使用new构造对象、数组
        public static Func<ASTree, Environment, object> Malloc
            = (ast, env) =>
            {
                string name = (ast[0] as VarName).Name;
                //ast[1][0] => postfix[0] => index
                int index1 = Convert.ToInt32(ast[1][0].Evaluate(env));
                var array = new object[index1];
                if (ast.ChildrenCount == 3)
                    array = array.Select(a => new object[Convert.ToInt32(ast[2][0].Evaluate(env))])
                                 .ToArray();
                return array;
            };

        //引用指令:
        //除非无意为之,一般情况下不会发生循环链接的错误
        //因为动态语言基本上不需要在使用一个类的时候
        //必须知道另一个类的具体定义(比如一个静态的Tables,
        //它维护着一些Table变量,但并不需要using Table,除了
        //要自己创建Table的实例时候)
        public static Func<ASTree, ASTree, Environment, object> Using
            = (name, list, env) =>
            {
                /*AppDomain.CurrentDomain.BaseDirectory + @"\"*/
                string fileName = name[0].Evaluate(env).ToString()
                       ,moduleName = fileName.Split('.')[0]
                       //无法使用Path.GetFullPath(".."),通过cmd调用程序会改变当前路径
                       , root = @"F:\Backup\backup\Code Warehouse\Projects\VS2015 Projects\KScript\KScript IDE\bin"
                       , dir = string.Format
                       (@"{0}\{1}\", root, fileName.Contains(".dll") ? "module" : "lib")
                       , path = dir + fileName + (fileName.Contains(".") ? "" : ".ks"); //默认是ks文件
                try
                {
                    //创建一个命名空间对象,可以通过命名空间显式调用其中的东西
                    KNameSpace knamespace = null;
                    if (File.Exists(path))
                    {
                        NestedEnv moduleEnv = new NestedEnv();
                        (env as NestedEnv).InsertEnv(moduleEnv);
                        knamespace = new KNameSpace(moduleEnv, moduleName);
                        EngineInitor.Initial(moduleEnv);
                        var eval = new Evaluator(File.ReadAllText(path));
                        //执行脚本,目的是创建对象和初始化数据,之后这些数据驻留在环境env中
                        eval.ExecuteWithExt(moduleEnv, false);
                        //var knamespace = new KNameSpace(moduleEnv, moduleName);
                        //env.PutInside(moduleName, knamespace);
                    }
                    else
                    {
                        knamespace = new KNameSpace(env, moduleName);
                        path = string.Format(@"{0}\module\{1}\{1}.dll", root, fileName.Split('.')[0]);
                        //加载拓展框架
                        EngineInitor.LoadNativeModule(knamespace, path);
                    }
                    //添加命名空间对象
                    if(list.ChildrenCount == 0)
                        env.PutInside(moduleName, knamespace);
                    else
                    {
                        foreach(var arg in list)
                        {
                            var _name = arg.Evaluate(env).ToString();
                            if (string.Compare(_name, "All", true) == 0)
                            {
                                knamespace.DumpInto(env);
                                break;
                            }
                            else knamespace.DumpInto(_name, env);
                        }
                    }
                }
                catch (Exception exc)
                {
                    throw new KException("Module loading error: " + fileName, name.LineNo);
                }
                return null;
            };

        public static Func<ASTree, Environment, int, object> Throw
            = (ast, env, loc) =>
            {
                throw new KException(ast.Evaluate(env).ToString(), ast.LineNo);
            };

        public static Func<ASTree, Environment, int, object> Assert
            = (ast, env, loc) =>
            {
                bool res = Convert.ToBoolean(ast.Evaluate(env));
                if(!res) throw new KException("Assertion failed!", loc);
                return 1D;
            };

        //对象移除指令
        //TODO:将变量封装成一个类,使用指针管理、
        //禁止删除、修改特定的对象、
        //并实现访问控制等操作
        public static object Delete(ASTree ast, Environment env)
        {
            object res = ast.Evaluate(env);
            if(res is KNameSpace)
            {
                var ns = res as KNameSpace;
                foreach (var varName in ns.Variables)
                {
                    //防止勿删被覆盖的成员 && env.Get(varName) == ns.Read(varName)
                    if (env.Contains(varName))
                    {
                        env.RemoveInside(varName);
                    }
                }
                env.RemoveInside(ns.Name);
            }
            //非命名空间对象
            else if (ast is VarName)
            {
                env.RemoveInside((ast as VarName).Name);
            }
            return null;
        }
    }
}
