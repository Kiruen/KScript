using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using KScript.Execution;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("Tuple", MapModifier.Static, MapType.CommonClass)]
    public class KTuple : KBuiltIn, Indexable, IEnumerable<object>
    {
        private object[] elements;

        public bool IsPair
        {
            get { return Count == 2; }
        }

        [MemberMap("count", MapModifier.Instance, MapType.Data)]
        public int Count
        {
            get { return elements.Length; }
        }

        public object this[object index]
        {
            get
            {
                return IndexParser.GetElementAt(index, Count, (i) => elements[i],
                                elements, (ienum) => new KTuple(ienum.ToArray()));
                //var res = IndexParser.ParseIndex(index, Count);
                //int start = res.Item1, len = res.Item2, step = res.Item3;
                //if (len == 0)
                //    return elements[start];
                //else if (step == 1)
                //    return new KTuple(elements.Skip(start).Take(len).ToArray());
                //else
                //{
                //    return new KTuple(elements.Skip(start)
                //                              .Take(len)
                //                              .Where((x, i) => i % step == 0)
                //                              .ToArray());
                //}
            }
            set { throw new KException("Can't change an invariable object!", Debugger.CurrLineNo); }
        }

        public KTuple(params object[] objs)
        {
            //var list = new List<object>();
            //foreach (var obj in objs)
            //{
            //    if (obj is KTuple)
            //        list.AddRange((obj as KTuple).elements);
            //    else
            //        list.Add(obj);
            //}
            //elements = list.ToArray();
            elements = objs;
        }

        public override KString ToStr()
        {
            return KUtil.ToString("(", elements, ")");
        }

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var val in elements)
                yield return val;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            else if (obj is KTuple)
            {
                var rtuple = (obj as KTuple).elements;
                if (rtuple.Length != elements.Length) return false;
                //elements.Zip((obj as KList), (x, y) => x.Equals(y)).Aggregate((x,y) => x && y);
                for (int i = 0; i < Count; i++)
                {
                    if (!rtuple[i].Equals(elements[i]))
                        return false;
                }
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
