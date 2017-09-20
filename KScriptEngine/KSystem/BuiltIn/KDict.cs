using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KScript.KSystem.BuiltIn
{
    /// <summary>
    /// 内建类:字典
    /// </summary>
    [MemberMap("Dict", MapModifier.Static, MapType.CommonClass)]
    public class KDict : KBuiltIn, Indexable, IEnumerable<object>
    {
        //间接使用HashCode作为查询依据
        private Dictionary<object, object> dict;

        [MemberMap("count", MapModifier.Instance, MapType.Data)]
        public double Count
        {
            get { return dict.Count; }
        }

        [MemberMap("keys", MapModifier.Instance, MapType.Data)]
        public object[] Keys
        {
            get { return dict.Keys.ToArray(); }
        }

        [MemberMap("values", MapModifier.Instance, MapType.Data)]
        public object[] Values
        {
            get { return dict.Values.ToArray(); }
        }

        public object this[object index]
        {
            get { return dict[index]; }
            set
            {
                if (dict.ContainsKey(index))
                    dict[index] = value;
                else
                    dict.Add(index, value);
            }
        }

        //static KMap()
        //{
        //    KUtil.FindFuncMapping(typeof(KMap), 
        //        (name, methodInfo) => funcCache.Add(name, methodInfo));
        //}

        [MemberMap("_cons", MapModifier.Instance, MapType.Constructor)]
        public KDict(int capacity = 8) 
            //: base(new NestedEnv())
        {
            dict = new Dictionary<object, object>(capacity);
            //KUtil.AddNatFuncFromMapping(funcCache, innerEnv, this);
            //添加拓展字段
            //innerEnv.PutInside("type", ClassLoader.GetClass("Dict"));
        }

        public KDict(Dictionary<object, object> dict)
        {
            this.dict = dict;
        }

        [MemberMap("has", MapModifier.Instance, MapType.Method)]
        public bool Contians(object obj)
        {
            return dict.ContainsKey(obj);
        }

        [MemberMap("clear", MapModifier.Instance, MapType.Method)]
        public void Clear()
        {
            dict.Clear();
        }

        [MemberMap("update", MapModifier.Instance, MapType.Method)]
        public void Update(KDict newMap)
        {
            newMap.dict.Keys
                .Select(key => this[key] = newMap[key])
                .ToArray();
        }

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var pair in dict)
                yield return pair;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override KString ToStr()
        {
            return KUtil.ToString("{", this, "}",
            p =>
            {
                var pair = (KeyValuePair<object, object>)p;
                return KUtil.ToString(pair.Key) + ":" + KUtil.ToString(pair.Value);
            });
            //var sb = new StringBuilder("{");
            //this.Select(p =>
            //{
            //    var pair = (KeyValuePair<object, object>)p;
            //    return sb.Append(pair.Key + ":" + pair.Value).Append(",");
            //}).ToArray();
            //if (sb.Length > 2)
            //    sb.Remove(sb.Length - 1, 1);
            //return sb.Append("}").ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            else if (obj is KDict)
            {
                var rdict = (obj as KDict).dict;
                if (rdict.Count != dict.Count) return false;
                //elements.Zip((obj as KList), (x, y) => x.Equals(y)).Aggregate((x,y) => x && y);
                foreach (var key in dict.Keys)
                    if (!(rdict.ContainsKey(key)
                        && rdict[key].Equals(dict[key])))
                        return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
