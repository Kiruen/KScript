using KScript.AST;
using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using MapModifier = KScript.KAttribute.MapModifier;

namespace KScript.KSystem.BuiltIn
{
    /// <summary>
    /// 内建类:字符串
    /// </summary>
    [MemberMap("Str", MapModifier.Static, MapType.CommonClass)]
    public class KString : KBuiltIn, IEnumerable<KString>
    {
        //private static Dictionary<string, MethodInfo> funcCache
        //    = new Dictionary<string, MethodInfo>(16);
        private readonly string val;
        public int Length
        {
            get { return val.Length; }
        }

        public KString this[object index]
        {
            get
            {
                return IndexParser.GetElementAt(index, Length, val);
            }
        }

        //加载好需要用到的原生函数的Info
        //static KString()
        //{
        //    KUtil.FindFuncMapping(typeof(KString),
        //        (name, methodInfo) => funcCache.Add(name, methodInfo));
        //}


        //创建一个自封闭的对象内环境(考虑到内建类不依赖外部环境的变量)
        public KString(object val = null)
            //: base(new NestedEnv())
        {
            this.val = val == null ? "" : val.ToString();
            //KUtil.AddNatFuncFromMapping(funcCache, innerEnv, this);
            //添加拓展字段
            innerEnv.PutInside("length", Length);
            //innerEnv.PutInside("type", ClassLoader.GetClass("Str"));
        }

        public KString(char[] chars)
            :this(new string(chars))
        { }

        [MemberMap("sub", MapModifier.Instance, MapType.Method)]
        public KString SubString(double start, double len)
        {
            int _start = (int)start, _len = (int)len;
            return start + _len < Length ? val.Substring(_start, _len)
                       : val.Substring(_start);
        }

        [MemberMap("trim", MapModifier.Instance, MapType.Method)]
        public KString Trim()
        {
            return val.Trim();
        }

        [MemberMap("split", MapModifier.Instance, MapType.Method)]
        public KString[] Split(KString sep)
        {
            return val.Split(sep.val.ToCharArray())
                      .Select(v => new KString(v))
                      .ToArray();
        }

        [MemberMap("has", MapModifier.Instance, MapType.Method)]
        public bool Contains(KString str)
        {
            return val.Contains(str.val);
        }

        [MemberMap("find", MapModifier.Instance, MapType.Method)]
        public double IndexOf(KString str)
        {
            return val.IndexOf(str);
        }

        [MemberMap("count", MapModifier.Instance, MapType.Method)]
        public double Count(KString str)
        {
            int len = str.Length;
            return (val.Length - val.Replace(str.val, "").Length) / len;
            //return Regex.Matches(val, str.val).Count;
        }

        [MemberMap("replace", MapModifier.Instance, MapType.Method)]
        public KString Replace(KString src, KString pattern)
        {
            return val.Replace(src, pattern);
        }

        [MemberMap("insert", MapModifier.Instance, MapType.Method)]
        public KString Insert(double start, KString pattern)
        {
            return val.Insert((int)start, pattern);
        }

        [MemberMap("join", MapModifier.Instance, MapType.Method)]
        public KString Join(IEnumerable<object> list)
        {
            return list.Select(obj => KUtil.ToString(obj))
                       .Aggregate((o1, o2) 
                        => KUtil.ToString(o1) + val + KUtil.ToString(o2));
        }

        [MemberMap("reverse", MapModifier.Instance, MapType.Method)]
        public KString Reverse()
        {
            return new string(val.Reverse().ToArray());
        }

        [MemberMap("format", MapModifier.Instance, MapType.Method)]
        public KString Format(IEnumerable<object> args)
        {
            return string.Format(val, args.ToArray());
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return val.Length == 0;
            else
                return val.Equals(obj.ToString());
        }

        public override string ToString()
        {
            return val;
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public override KString ToStr()
        {
            return ToString();
            //return string.Format("\"{0}\"", ToString());
        }

        public IEnumerator<KString> GetEnumerator()
        {
            return val.Select(ch => new KString(ch))
                      .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        //适配模式:
        public static KString operator +(KString a, KString b)
        {
            return new KString(a.val + b.val);
        }

        public static KString operator -(KString a, KString b)
        {
            return new KString(a.val.Replace(b.val, ""));
        }

        public static bool operator ==(KString a, KString b)
        {
            return a.val.Equals(b.val);
        }

        public static bool operator !=(KString a, KString b)
        {
            return !(a == b);
        }

        public static implicit operator string(KString kstr)
        {
            return kstr.val;
        }
        
        public static implicit operator KString(string str)
        {
            return new KString(str);
        }

        public static KString Instance(object val)
        {
            return new KString(val);
        }
    }
}
