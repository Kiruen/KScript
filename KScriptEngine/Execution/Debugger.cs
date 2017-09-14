using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KScript.Execution
{
    public enum Actions
    {
        StepAllBeforeHit = 1,
        StepOver = 2,
        StepIn = 3,
        StepReturn = 4,
    }

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
        public static Stack<string> CallStack { get; set; }
                                        = new Stack<string>(32);

        public static int DebuggingStackLevel { get; set; }
        public static int StackLevel
        {
            get { return CallStack.Count; }
        }

        public static int CurrLineNo { get; set; }

        public static Action<int, Environment> OnUpdate { get;set; }

        public static void Run(HashSet<int> breakPoints, ThreadStart actions)
        {
            CallStack.Clear();
            //注意,此变量引用的是text的变量
            WillStepOver = false;
            DebuggingStackLevel = 1;
            CurrLineNo = 0;

            BreakPoints = breakPoints;
            TCProgram = new Thread(actions);
            TCProgram.Start();
        }

        public static void UpdateData(int lineNo, Environment env)
        {
            CurrLineNo = lineNo;
            VarTable = env;
            //更新回调
            OnUpdate(lineNo, env);
        }

        public static void PushFunc(string funcInfo)
        {
            if (DebuggingStackLevel <= 1024)
            {
                CallStack.Push(funcInfo);
                if (WillStepIn)
                    DebuggingStackLevel = StackLevel;
            }
            else
                throw new KException("Stack overflow!", 0);
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
            }
            //如果期望步入,则更新当前调试层(变为下一层/不变(无函数可步入))
            else if (WillStepIn)
                DebuggingStackLevel = StackLevel;
            //尝试挂起程序执行线程
            if (WillStepOver && DebuggingStackLevel == StackLevel || WillStepIn)
                TCProgram.Suspend();
        }

        public static void GoOn(Actions next = Actions.StepAllBeforeHit)
        {
            if (TCProgram?.ThreadState == ThreadState.Suspended)
            {
                TCProgram.Resume();
                if (next == Actions.StepOver)
                {
                    WillStepOver = true;
                    WillStepIn = false;
                    //DebuggingStackLevel = StackLevel;
                }
                else if (next == Actions.StepAllBeforeHit)
                {
                    WillStepOver = false;
                    WillStepIn = false;
                }
                else if (next == Actions.StepIn)
                {
                    WillStepOver = false;
                    WillStepIn = true;
                }
                else if (next == Actions.StepReturn)
                {
                    WillStepOver = true;
                    WillStepIn = false;
                    DebuggingStackLevel--;
                }
            }
        }

        public static void Stop()
        {
            GoOn();
            if (TCProgram?.ThreadState != ThreadState.Aborted)
                TCProgram?.Abort();
        }
    }
}
