using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace KScript
{
    public static class KStdInputStream
    {
        public static bool Inputing { get; private set; }
        public static StringBuilder Buffer { get; private set; }

        static KStdInputStream()
        {
            Buffer = new StringBuilder(128);
        }

        public static string ReadLine()
        {
            Begin();
            while (Inputing)
            {
                Thread.Sleep(50);
            }
            //自行end
            var content = Buffer.ToString();
            return content.Length > 1 ? content.Remove(content.Length - 1) : null;
        }

        public static string ReadKey()
        {
            Begin();
            while (Inputing && Buffer.Length == 0)
            {
                Thread.Sleep(100);
            }
            End();
            return Buffer.ToString();
        }

        public static string ReadKeyAsyn()
        {
            Begin();
            if(Buffer.Length == 0)
                Thread.Sleep(500);
            End();
            return Buffer.ToString();
        }

        public static void Begin()
        {
            Inputing = true;
            Buffer.Clear();
        }

        public static void End()
        {
            Inputing = false;
        }

        public static void Push(object content)
        {
            Buffer.Append(content);
        }
    }
}
