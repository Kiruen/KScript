using KScript.AST;
using KScript.KSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KScript
{
    /// <summary>
    /// 一个控制台外壳类,用于封装对控制台的操作
    /// </summary>
    public static class KConsole
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        private static bool Active { get; set; }
        private static IntPtr chWnd;
        private static IntPtr xhWnd;

        public static void Show()
        {
            if (!Active)
            {
                Active = true;
                AllocConsole();
                chWnd = GetConsoleWindow();
                xhWnd = GetSystemMenu(chWnd, IntPtr.Zero);
                RemoveMenu(xhWnd, 0xF060, 0x0);
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else ShowWindow(chWnd, 1);
        }

        public static void Close()
        {
            //FreeConsole();
            ShowWindow(chWnd, 0);
        }

        public static void AnyKeyToExit()
        {
            Console.Beep();
            WriteLine("Press any key to exit...");
            if (Read() != string.Empty)
            {
                Clear();
                Close();
            }
        }

        public static void WriteLine(params object[] objs)
        {
            Console.WriteLine(objs
                .Select(o => KUtil.ToString(o))
                .Aggregate((s1, s2) =>  s1 + ' ' + s2));
        }

        public static void Write(params object[] objs)
        {
            foreach (var obj in objs)
                Console.Write(KUtil.ToString(obj) + ' ');
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static string Read()
        {
            return Console.ReadKey().KeyChar + "";
        }

        public static string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
