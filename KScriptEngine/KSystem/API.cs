using KScript.AST;
using KScript.Callable;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;
using KScript.Utils;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KScript.KSystem
{
    [MemberMap("StdLib", MapModifier.Static, MapType.ToolClass)]
    public static class API
    {
        private static Random random = new Random(12);

        [MemberMap("alert", MapModifier.Static, MapType.Method)]
        public static void Alert(object obj)
        {
            MessageBox.Show(obj.ToString());
        }

        [MemberMap("sleep", MapModifier.Static, MapType.Method)]
        public static void Sleep(int time)
        {
            Thread.Sleep(time);
        }

        [MemberMap("write", MapModifier.Static, MapType.Method, true)]
        public static void Write(params object[] obj)
        {
            //Console.Text += obj + "";
            //Console.Select(Console.Text.Length, Console.Text.Length);
            //Console.ScrollToCaret();
            KConsole.Write(obj);
        }

        [MemberMap("print", MapModifier.Static, MapType.Method, true)]
        public static void WriteLine(params object[] obj)
        {
            //Console.Text += obj + "\r\n";
            //Console.Select(Console.Text.Length, Console.Text.Length);
            //Console.ScrollToCaret();
            KConsole.WriteLine(obj);
        }

        [MemberMap("read", MapModifier.Static, MapType.Method)]
        public static KString Read()
        {
            //return Interaction.InputBox("Console Input");
            //return KStdInputStream.ReadKey();
            return KConsole.Read();
        }

        [MemberMap("readAsyn", MapModifier.Static, MapType.Method)]
        public static KString ReadAsyn()
        {
            //return Interaction.InputBox("Console Input");
            return KStdInputStream.ReadKeyAsyn();
        }

        [MemberMap("readLine", MapModifier.Static, MapType.Method)]
        public static KString ReadLine()
        {
            //return Interaction.InputBox("Console Input");
            //return KStdInputStream.ReadLine();
            return KConsole.ReadLine();
        }

        [MemberMap("clear", MapModifier.Static, MapType.Method)]
        public static void Clear()
        {
            KConsole.Clear();
        }

        [MemberMap("eval", MapModifier.Static, MapType.Method)]
        public static object Eval(KString code)
        {
            Lexer lexer = new Lexer(code);
            Environment env = new NestedEnv();
            BasicParser parser = new BasicParser();
            object result = null;
            while (lexer.Peek(0) != Token.EOF)
            {
                ASTree ast = parser.Parse(lexer);
                if (!(ast is NullStmnt))
                    result = ast.Evaluate(env);
            }
            return result;
        }

        [MemberMap("currentTime", MapModifier.Static, MapType.Method)]
        public static KString CurrentTime()
        {
            return DateTime.Now.ToString();
        }

        [MemberMap("random", MapModifier.Static, MapType.Method)]
        public static double Random(double min, double max)
        {
            return random.Next((int)min, (int)max);
        }

        [MemberMap("random", MapModifier.Static, MapType.Method)]
        public static double Random()
        {
            return random.NextDouble();
        }

        [MemberMap("toNum", MapModifier.Static, MapType.Method)]
        [MemberMap("toNumber", MapModifier.Static, MapType.Method)]
        public static double ToNumber(object val)
        {
            return double.Parse(val.ToString());
        }

        //类型检测函数
        [MemberMap("typeof", MapModifier.Static, MapType.Method)]
        public static ClassInfo TypeOf(object obj)
        {
            if (obj == null) return ClassLoader.GetClass("None");
            else if (obj is double)
                return ClassLoader.GetClass("Num");
            else if (obj.GetType().IsArray)
                return ClassLoader.GetClass("Arr");
            else if (obj is IFunction)
                return ClassLoader.GetClass("Callable");
            else if (obj is KObject)
                return (obj as KObject).Read<ClassInfo>("type");
            //else if (obj is ClassInfo)
            //    return ClassLoader.GetClass("Type");
            else
                return ClassLoader.GetOrCreateClass(obj.GetType().Name);
        }

        [MemberMap("toStr", MapModifier.Static, MapType.Method)]
        public static KString ToStr(object obj)
        {
            return KUtil.ToString(obj);
        }

        [MemberMap("dict", MapModifier.Static, MapType.Method)]
        public static KDict Dict()
        {
            return new KDict();
        }

        [MemberMap("tup", MapModifier.Static, MapType.Method, true)]
        public static KTuple Tuple(params object[] objs)
        {
            return new KTuple(objs);
        }

        [MemberMap("toBig", MapModifier.Static, MapType.Method)]
        public static KString ToBig(object num)
        {
            string str_num = num.ToString();
            if (str_num.Contains("E") || str_num.Contains("e"))
            {
                var parts = str_num.Split('.', 'E', 'e');
                int dot, suffix = int.Parse(parts[2]);
                StringBuilder sb = new StringBuilder(parts[0]);
                for (dot = 0; dot < suffix; dot++)
                {
                    if (dot < parts[1].Length)
                        sb.Append(parts[1][dot]);
                    else sb.Append(0);
                }
                if (dot < parts[1].Length)
                {
                    sb.Append('.');
                    sb.Append(new string(parts[1].Skip(dot).ToArray()));
                }     
                str_num = sb.ToString();
            }
            return str_num;
        }

        [MemberMap("exDiv", MapModifier.Static, MapType.Method)]
        public static double ExDiv(int a, int b)
        {
            return a / b;
        }

        [MemberMap("pickFunc", MapModifier.Static, MapType.Method)]
        public static IFunction PickFunc(IFunction fun, int paramLen)
        {
            return fun[paramLen] ?? 
                throw new Exception("No such override version!");
        }

        //public static object Break()
        //{
        //    return new PartiToken("break", null);
        //}

        //public static object SuperBreak(int nest)
        //{
        //    return new PartiToken("superbreak", null);
        //}

        //public static object Continue()
        //{
        //    return new PartiToken("continue", null);
        //}

        //public static object Return(object arg)
        //{
        //    return new PartiToken("return", arg);
        //}
    }
}
