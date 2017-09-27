using KScript.AST;
using KScript.KSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32.SafeHandles;
using KScript.Utils;
using System.Windows.Forms;

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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
                                  string lpFileName,
                                  uint dwDesiredAccess,
                                  uint dwShareMode,
                                  uint lpSecurityAttributes,
                                  uint dwCreationDisposition,
                                  uint dwFlagsAndAttributes,
                                  uint hTemplateFile);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern int SetWindowText(IntPtr hwnd, string lpString);

        private static bool Active { get; set; }
        private static IntPtr chWnd;
        private static IntPtr xhWnd;

        private const int MY_CODE_PAGE = 437;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_WRITE = 0x2;
        private const uint OPEN_EXISTING = 0x3;

        public static void Show(string title = "")
        {
            if (!Active)
            {
                Active = true;
                AllocConsole();
                //IntPtr stdHandle = CreateFile("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
                //SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
                //FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                //Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                //StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                //standardOutput.AutoFlush = true;
                //Console.SetOut(standardOutput);
                
                chWnd = GetConsoleWindow();
                xhWnd = GetSystemMenu(chWnd, IntPtr.Zero);
                RemoveMenu(xhWnd, 0xF060, 0x0);
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                ShowWindow(chWnd, 1);
            SetWindowText(chWnd, title);
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
                    .DefaultIfEmpty("")
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
