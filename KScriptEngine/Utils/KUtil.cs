using KScript.AST;
using KScript.Callable;
using KScript.Runtime;
using KScript.KAttribute;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace KScript.Utils
{
    /*
        Environment.SetEnvironmentVariable("KS_PATH", "%JAVA_HOME%;", EnvironmentVariableTarget.User);
        Console.WriteLine(Environment.GetEnvironmentVariable("KS_PATH", EnvironmentVariableTarget.User));
    */
    public static class IndexParser
    {
        private static int ParseSingle(object val, int maxLen)
        {
            if (val.GetType().IsValueType)
            {
                int num = Convert.ToInt32(val);
                if (num >= maxLen) return maxLen;
                else if (num < -maxLen) return 0;
                else return (maxLen + num) % maxLen;
            }
            else if (val.Equals("end"))
                return maxLen;
            else if (val.Equals("start"))
                return -1;
            return 0;
        }

        /// <summary>
        /// 转换索引(tuple、str、num)
        /// </summary>
        /// <param name="index">表示索引的对象</param>
        /// <param name="maxLen">线性表的最大长度</param>
        /// <returns></returns>
        public static Tuple<int, int, int> ParseIndex(object index, int maxLen)
        {
            if (index != null)
            {
                if (index.GetType().IsValueType)
                {
                    int _index = Convert.ToInt32(index);
                    if (_index < maxLen && _index >= -maxLen)
                        return Tuple.Create((maxLen + Convert.ToInt32(index)) % maxLen, 0, 0);
                    else throw new KException("Index out of bound!", Debugger.CurrLineNo);
                }
                else if (index is KTuple)
                {
                    var tuple = index as KTuple;
                    int start = ParseSingle(tuple[0], maxLen),
                        end = ParseSingle(tuple[1], maxLen),
                        len = end - start, 
                        step = 1;
                    //if (end < 0) len = (maxLen + end - start) % maxLen;
                    if (tuple.Count >= 3)
                        step = Convert.ToInt32(tuple[2]);
                    return Tuple.Create(start, len, step);
                }
            }
            throw new KException("Invalid index!", Debugger.CurrLineNo);
        }

        public static object GetElementAt(object index, int maxLen, Func<int, object> sinIndexer, IEnumerable<object> source, Func<IEnumerable<object>, object> creator)
        {
            var res = ParseIndex(index, maxLen);
            int start = res.Item1, len = res.Item2, step = res.Item3;
            if (len == 0)
                return sinIndexer(start);
            else
            {
                IEnumerable<object> elements = source;
                if (len < 0)
                {
                    elements = elements.Reverse();
                    start = maxLen - start - 1;
                    len = -len;
                }
                if (step == 1)
                    return creator(elements.Skip(start).Take(len));
                else
                {
                    return creator(elements.Skip(start).Take(len)
                                             //可使用提供索引值的func
                                           .Where((x, i) => i % step == 0));
                }
            }
        }

        public static KString GetElementAt(object index, int maxLen, string obj)
        {
            var res = ParseIndex(index, maxLen);
            int start = res.Item1, len = res.Item2, step = res.Item3;
            if (len == 0)
                return KString.Instance(obj[start]);
            else
            {
                IEnumerable<char> chars = obj;
                if (len < 0)
                {
                    chars = chars.Reverse();
                    start = maxLen - start - 1;
                    len = -len;
                }
                if (step == 1)
                    return KString.Instance(chars.Skip(start).Take(len).ToArray());
                else
                {
                    return KString.Instance(chars.Skip(start).Take(len)
                                           //可使用提供索引值的func
                                           .Where((x, i) => i % step == 0)
                                           .ToArray());
                }
            }
        }
    }

    public static class KUtil
    {
        /// <summary>
        /// 加载程序集,并将其目录下所有的相关程序集一并导入
        /// (原问题已解决,不建议再使用)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Assembly LoadDll(string path)
        {
            var dir = Path.GetDirectoryName(path);
            var asm = Assembly.LoadFrom(path);
            //var catalog = new AggregateCatalog();
            //catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFrom("dll1的路径");
            //catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFrom("dll2的路径"));
            //var container = new CompositionContainer(catalog);
            //CompositionBatch batch = new CompositionBatch();

            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    string strFielName = args.Name.Split(',')[0];
            //    return Assembly.LoadFile($"{dir}\\{strFielName}.dll");
            //};

            //foreach (var file in Directory.GetParent(path)
            //                            .GetFiles()
            //                            .Where(f => f.Extension == "dll"))
            //{
            //    currentDomain.AssemblyResolve += (sender, args) =>
            //    {
            //        string strFielName = args.Name.Split(',')[0];
            //        return Assembly.LoadFile(string.Format($"{file.DirectoryName}\\{file.Name}", strFielName));
            //    };
            //}
            return asm;
        }

        public static string Formalize(string str)
        {
            return char.ToLower(str[0]) + str.Substring(1, str.Length - 1);
        }

        public static string ToString(object obj)
        {
            if (obj != null)
            {
                if (obj is string || obj.GetType().IsValueType)
                    return obj.ToString();
                else if (obj is KObject)
                    return (obj as KObject).ToStr();
                else if (obj is IEnumerable<object>)
                {
                    return ToString("[", obj as IEnumerable<object>, "]");
                }
                else return obj.GetType().Name;
            }
            return string.Empty;
        }

        public static string ToString(string leftSep, IEnumerable<object> objs, string rightSep, Func<object, string> map = null)
        {
            var it = objs as IEnumerable<object>;
            var sb = new StringBuilder(leftSep);
            if (it.Count() > 0)
                sb.Append(
                    it.Select(map != null ? map :
                    s =>
                    {
                        if (s == null) return "None";
                        else return ToString(s);
                    })
                    .Aggregate((x, y) => x + ", " + y)
                );
            return sb.Append(rightSep).ToString();
        }
        /// <summary>
        /// 从类信息中寻找原生类型的成员映射,支持单一元素的多个特性描述
        /// </summary>
        /// <param name="type">搜索的类</param>
        /// <param name="action">每次搜索到一个映射函数后执行的动作</param>
        public static void FindMapping(Type type, Action<MemberMapAttribute, MemberInfo> action)
        {
            var members = type.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);   // | BindingFlags.DeclaredOnly
            foreach (var member in members)
            {
                //寻找此成员的所有特性(多对一映射,可以保留旧的API)
                foreach(var attr in member.GetCustomAttributes<MemberMapAttribute>())
                {
                    action(attr, member);
                }
                //var attr = member.GetCustomAttribute(typeof(MemberMapAttribute))
                //                                          as MemberMapAttribute;
                //if (attr != null)
                //    action(attr.MappingName, member);
            }
        }

        /// <summary>
        /// 从反射信息缓存集中获取对象并进行操作
        /// </summary>
        /// <param name="infoCache">反射信息缓存集</param>
        /// <param name="action">每获取到一个对象时的操作</param>
        public static void AddNatMemberFromMapping(IList<MemberInfo> infoCache, Action<MemberMapAttribute, MemberInfo> action)
        {
            foreach (var info in infoCache)
            {
                action(info.GetCustomAttribute<MemberMapAttribute>(), info);
            }
        }
    }
}
