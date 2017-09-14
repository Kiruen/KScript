using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace KScript
{
    public class KTimeOut
    {
        private TimeSpan timeOut = new TimeSpan(0, 0, 5);
        //private AutoResetEvent autoEvent 
        //                    = new AutoResetEvent(false);
        private Action work;

        public KTimeOut(TimeSpan timeout, Action work)
        {
            timeOut = timeout;
            this.work = work;
        }

        public KTimeOut(int seconds, Action work)
        {
            timeOut = new TimeSpan(0, 0, 0, seconds);
            this.work = work;
        }

        public void Run()
        {
            //ThreadPool.QueueUserWorkItem(Work, autoEvent);
            //autoEvent.WaitOne(timeOut, false);
            //Block for a timeout
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            var task = Task.Run(work, token);
            try
            {
                while (!task.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(200);
                }
            }
            catch
            {
                //Console.WriteLine("Program timeout!!!");
            }
            //cts.Token.Register(() =>
            //{

            //});
        }

        private void Work(object stateInfo)
        {
            //Do something
            work();
            //If end, give signal, otherwise you will wait for timeout
            ((AutoResetEvent)(stateInfo)).Set();
        }
    }
}
