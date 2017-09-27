using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KScript.Utils
{
    public static class KeyCollector
    {
        private class Filter : IMessageFilter
        {
            public bool IsWorking { get; set; }
            public string Key { get; set; }

            public bool PreFilterMessage(ref Message m)
            {
                if (IsWorking && m.Msg == 0x0100)
                    Key = m.LParam.ToString();
                return false;
            }
        }

        private static Filter collector = new Filter();
        static KeyboardHook hook;
        static KeyCollector()
        {
            hook = new KeyboardHook();
            hook.SetHook();
        }

        public static bool Collected
        {
            get { return collector.Key != null; }
        }

        public static string Key
        {
            get { return collector.Key; }
        }

        public static void Registory()
        {
            Application.AddMessageFilter(collector);
        }

        public static void Start()
        {
            collector.IsWorking = true;
            collector.Key = null;
        }

        public static void End()
        {
            collector.IsWorking = false;
        }
    }
}
