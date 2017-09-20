using KScript.KAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    /// <summary>
    /// 内建类:字典
    /// </summary>
    [MemberMap("Set", MapModifier.Static, MapType.CommonClass)]
    public class KSet : KBuiltIn, IEnumerable<object>
    {
        //间接使用HashCode作为查询依据
        private ISet<object> set;

        [MemberMap("count", MapModifier.Instance, MapType.Data)]
        public double Count
        {
            get { return set.Count; }
        }

        [MemberMap("values", MapModifier.Instance, MapType.Data)]
        public object[] Values
        {
            get { return set.ToArray(); }
        }

        //static KMap()
        //{
        //    KUtil.FindFuncMapping(typeof(KMap), 
        //        (name, methodInfo) => funcCache.Add(name, methodInfo));
        //}

        [MemberMap("_cons", MapModifier.Instance, MapType.Constructor)]
        public KSet(int sorted = 0)
        {
            if (sorted == 1)
                set = new SortedSet<object>();
            else
                set = new HashSet<object>();
        }

        public KSet(IEnumerable<object> coll)
            :this(0)
        {
            foreach (var val in coll)
                set.Add(val);
        }

        [MemberMap("add", MapModifier.Instance, MapType.Method)]
        public void Add(object obj)
        {
            set.Add(obj);
        }

        [MemberMap("remove", MapModifier.Instance, MapType.Method)]
        public void Remove(object obj)
        {
            set.Remove(obj);
        }

        [MemberMap("has", MapModifier.Instance, MapType.Method)]
        public bool Contians(object obj)
        {
            return set.Contains(obj);
        }

        [MemberMap("clear", MapModifier.Instance, MapType.Method)]
        public void Clear()
        {
            set.Clear();
        }

        [MemberMap("except", MapModifier.Instance, MapType.Method)]
        public KSet Except(KSet rset, int newInstance = 1)
        {
            if (newInstance == 1)
                return new KSet(set.Except(rset.set));
            else
            {
                set.ExceptWith(rset.set);
                return this;
            }       
        }

        [MemberMap("_sub", MapModifier.Instance, MapType.Method)]
        public KSet _Sub(KSet rset)
        {
            return Except(rset);
        }

        [MemberMap("intersect", MapModifier.Instance, MapType.Method)]
        public KSet Intersect(KSet rset, int newInstance = 1)
        {
            if (newInstance == 1)
                return new KSet(set.Intersect(rset.set));
            else
            {
                set.IntersectWith(rset.set);
                return this;
            }
        }

        [MemberMap("union", MapModifier.Instance, MapType.Method)]
        public KSet Union(KSet rset, int newInstance = 1)
        {
            if (newInstance == 1)
                return new KSet(set.Union(rset.set));
            else
            {
                set.UnionWith(rset.set);
                return this;
            }
        }

        [MemberMap("include", MapModifier.Instance, MapType.Method)]
        public bool Include(KSet rset)
        {
            return set.IsSubsetOf(rset.set);
        }

        [MemberMap("propInclude", MapModifier.Instance, MapType.Method)]
        public bool PropInclude(KSet rset)
        {
            return set.IsProperSubsetOf(rset.set);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override KString ToStr()
        {
            return KUtil.ToString("{", this, "}");
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            else if (obj is KSet)
            {
                var rset = (obj as KSet).set;
                return set.SetEquals(rset);
                //if (rset.Count != set.Count) return false;
                ////不支持特殊对象
                //foreach (var val in set)
                //    if (!rset.Contains(val))
                //        return false;
                //return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
