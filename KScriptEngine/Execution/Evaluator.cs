using KScript.AST;
using KScript.Execution;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KScript
{
    public partial class Evaluator
    {
        private static BasicParser parser = new ArrayParser();
        private Environment execEnv { get; set; }

        private Lexer lexer;
        private StringBuilder scriptTemp;
        public double TimeSpan { get; private set; }

        public Evaluator(string script)
        {
            lexer = new Lexer(script);
            lexer.ReadAll();
        }

        public double GetNumber(string varName)
        {
            return Convert.ToDouble(execEnv.Get(varName));
        }

        public object GetVariable(string varName)
        {
            return execEnv.Get(varName);
        }

        public void SetVariable(string varName, object obj)
        {
            execEnv.Put(varName, obj);
        }

        /// <summary>
        /// 执行脚本程序,且进行附加的操作,并返回最后结果
        /// </summary>
        /// <param name="env">执行脚本的环境</param>
        /// <param name="cleanClass">指明是否清理类自定义信息的缓存</param>
        /// <returns></returns>
        public object ExecuteWithExt(Environment env = null, bool cleanClass = true, bool isMain = false)
        {
            DateTime startTime = DateTime.Now;
            execEnv = (env == null ? new NestedEnv() : env);
            //NativeObject.CreateNativeObjs(env);
            EngineInitor.Initial(execEnv);
            if(isMain)
                execEnv.PutInside("main", new KNameSpace(execEnv, "main"));
            object result = Execute(execEnv);
            //清空类信息缓存
            if(cleanClass)
                ClassLoader.Clear();
            TimeSpan span = DateTime.Now - startTime;
            TimeSpan = span.TotalMilliseconds;
            return result;
        }

        /// <summary>
        /// 执行脚本程序,不进行任何附加操作(如标准API加载等),并返回结果
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public object Execute(Environment env)
        {
            object result = null;
            while (lexer.TokenCount != 0)
            {
                ASTree ast = Parse(lexer);
                if (!(ast is NullStmnt))
                {
                    Debugger.UpdateData(ast.LineNo, env);
                    Debugger.TrySuspend();
                    result = ast.Evaluate(env);
                    if (result is SpecialToken)
                    {
                        break;
                    } 
                }
            }
            return result;
        }


        public string[] GetDataTemp()
        {
            return execEnv.Names;
        }

        /// <summary>
        /// 进行语法分析
        /// </summary>
        /// <param name="lexer"></param>
        /// <returns></returns>
        public static ASTree Parse(Lexer lexer)
        {
            return parser.Parse(lexer);
        }
    }
}
