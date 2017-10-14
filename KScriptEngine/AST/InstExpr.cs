using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using KScript.Utils;
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
                    return new InstToken(InstName, null);
                case "return": return new InstToken(InstName, this[1].Evaluate(env));
                case "new": return InstFactory.Malloc(MainArg, env);
                case "using": return InstFactory.Using(MainArg, DeputyArg, env);
                case "del": return InstFactory.Delete(MainArg, env);
                case "throw": InstFactory.Throw(MainArg, env); return null;
                case "assert": return InstFactory.Assert(MainArg, env);
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
        public static object Malloc(ASTree ast, Environment env)
        {
            string name = (ast[0] as VarName).Name;
            //ast[1][0] => postfix[0] => index
            int index1 = Convert.ToInt32(ast[1][0].Evaluate(env));
            var array = new object[index1];
            if (ast.ChildrenCount == 3)
                array = array.Select(a => new object[Convert.ToInt32(ast[2][0].Evaluate(env))])
                             .ToArray();
            return array;
        }

        //引用指令:
        //除非无意为之,一般情况下不会发生循环链接的错误
        //因为动态语言基本上不需要在使用一个类的时候
        //必须知道另一个类的具体定义(比如一个静态的Tables,
        //它维护着一些Table变量,但并不需要using Table,除了
        //要自己创建Table的实例时候)
        public static object Using(ASTree linkName, ASTree list, Environment env)
        {
            /*AppDomain.CurrentDomain.BaseDirectory + @"\"*/
            string fileName = linkName[0].Evaluate(env).ToString(),
                moduleName = KPath.GetModuleName(fileName),
                dir = null, path = null;
            if (!fileName.Contains(".")) fileName += ".ks";
            if (File.Exists(fileName))
            {
                moduleName = fileName;
                path = fileName;
            }
            else
            {
                //无法使用Path.GetFullPath(".."),通过cmd调用程序会改变当前路径
                dir = fileName.Contains(".dll") ? KPath.MoudleRoot : KPath.LibRoot;
                path = Path.Combine(dir, fileName); //默认是ks文件
            }
            try
            {
                //创建一个命名空间对象,可以通过命名空间显式调用其中的东西
                KNameSpace knamespace = null;
                if (File.Exists(path))
                {
                    NestedEnv moduleEnv = new NestedEnv();
                    (env as NestedEnv).InsertEnv(moduleEnv);
                    knamespace = new KNameSpace(moduleName, moduleEnv);
                    //初始化模块作用域,提供模块初始化必备的数据
                    EngineInitor.Initial(moduleEnv);
                    var eval = new Evaluator(File.ReadAllText(path));
                    //执行目标文件的代码,目的是创建对象和初始化数据,之后这些数据驻留在环境env中
                    eval.Execute(moduleEnv, false);
                    //var knamespace = new KNameSpace(moduleEnv, moduleName);
                    //env.PutInside(moduleName, knamespace);
                }
                else
                {
                    knamespace = new KNameSpace(moduleName, env);
                    path = Path.Combine(KPath.MoudleRoot, moduleName, moduleName + ".dll");
                    //加载拓展框架
                    EngineInitor.LoadNativeModule(knamespace, path);
                }
                //添加命名空间对象
                if (list.ChildrenCount == 0)
                    env.PutInside(moduleName, knamespace);
                else
                {
                    foreach (var arg in list)
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
                throw new KException($"Module loading error: {fileName}\n" +
                                     $"Source error:\n{exc.Message}", linkName.LineNo);
            }
            return null;
        }

        public static void Throw(ASTree ast, Environment env)
        {
            throw new KException(ast.Evaluate(env).ToString(), ast.LineNo);
        }

        public static object Assert(ASTree ast, Environment env)
        {
            bool res = Convert.ToBoolean(ast.Evaluate(env));
            if (!res) throw new KException("Assertion failed!", ast.LineNo);
            return 1D;
        }

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
