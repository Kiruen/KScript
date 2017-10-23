using KScript.AST;
using KScript.Callable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KScript.Runtime
{
    public enum DebugActions
    {
        StepAllBeforeHit = 1,
        StepOver = 2,
        StepIn = 3,
        StepReturn = 4,
    }

    /// <summary>
    /// 脚本系统内置调试器
    /// </summary>
    public static class Debugger
    {
        public static Thread TCProgram { get; set; }
        public static Environment VarTable { get; set; }

        public static bool WillStepIn { get; set; }
        public static bool WillStepOver { get; set; }
        public static bool Debugging { get; set; }
        public static bool Hitted
        {
            get { return BreakPoints.Contains(CurrLineNo); }
        }

        public static HashSet<int> BreakPoints { get; set; }
                                        = new HashSet<int>();
        public static Stack<IFunction> CallStack { get; set; }
                                        = new Stack<IFunction>(32);

        public static int DebuggingStackLevel { get; set; }
        public static int StackLevel
        {
            get { return CallStack.Count; }
        }

        public static int CurrLineNo { get; set; }

        public static IFunction CurrentFunc
        {
            get { return CallStack.Count == 0 ? 
                        null : CallStack.Peek(); }
        }

        public static Action<int, Environment> OnUpdate { get;set; }

        public static void Run(HashSet<int> breakPoints, ThreadStart actions)
        {
            CallStack.Clear();
            
            WillStepOver = false;
            Debugging = false;
            DebuggingStackLevel = 1;
            CurrLineNo = 0;
            //注意,此变量引用的是text的变量
            BreakPoints = breakPoints;
            TCProgram = new Thread(actions);
            TCProgram.Start();
        }

        public static void UpdateData(int lineNo, Environment env)
        {
            CurrLineNo = lineNo;
            VarTable = env;
            //更新回调
            //2017-10-23 18:04:34 新增了&& DebuggingStackLevel == StackLevel
            if (Debugging && DebuggingStackLevel == StackLevel)
                OnUpdate(lineNo, env);
        }

        public static void PushFunc(IFunction funcInfo)
        {
            if (DebuggingStackLevel <= 1024)
            {
                CallStack.Push(funcInfo);
                if (WillStepIn)
                    DebuggingStackLevel = StackLevel;
            }
            else
                throw new KException("Stack overflow!", CurrLineNo);
        }

        public static void PopFunc()
        {
            //如果正在调试当前层,则退出到当前层上一层
            if (DebuggingStackLevel == StackLevel)
                DebuggingStackLevel--;
            CallStack.Pop();
        }

        public static void TrySuspend()
        {
            //如果击中断点,则进入步入模式,并更新当前调试层
            if (Hitted)
            {
                WillStepOver = true;
                WillStepIn = false;
                DebuggingStackLevel = StackLevel;
                Debugging = true;
                OnUpdate(CurrLineNo, VarTable);
            }
            //如果期望步入,则更新当前调试层(变为下一层/不变(无函数可步入))
            else if (WillStepIn)
                DebuggingStackLevel = StackLevel;
            //尝试挂起执行脚本的线程
            if (WillStepOver && DebuggingStackLevel == StackLevel || WillStepIn)
                TCProgram.Suspend();
        }

        public static void Continue(DebugActions next = DebugActions.StepAllBeforeHit)
        {
            if (TCProgram?.ThreadState == ThreadState.Suspended)
            {
                TCProgram.Resume();
                if (next == DebugActions.StepOver)
                {
                    WillStepOver = true;
                    WillStepIn = false;
                    //DebuggingStackLevel = StackLevel;
                }
                else if (next == DebugActions.StepAllBeforeHit)
                {
                    WillStepOver = false;
                    WillStepIn = false;
                    Debugging = false;
                }
                else if (next == DebugActions.StepIn)
                {
                    WillStepOver = false;
                    WillStepIn = true;
                }
                else if (next == DebugActions.StepReturn)
                {
                    WillStepOver = true;
                    WillStepIn = false;
                    DebuggingStackLevel--;
                }
            }
        }

        public static void Stop()
        {
            Continue();
            if (TCProgram?.ThreadState != ThreadState.Aborted)
                TCProgram?.Abort();
        }
    }
}
