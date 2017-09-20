using KScript.Callable;
using KScript.KAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("Gener", MapModifier.Static, MapType.CommonClass)]
    public class KGenerator : KBuiltIn, IEnumerable<object>
    {
        private IEnumerable<object> elements;

        [MemberMap("_cons", MapModifier.Instance, MapType.Constructor)]
        public KGenerator(IEnumerable<object> elements)
        {
            this.elements = elements;
        }

        [MemberMap("where", MapModifier.Instance, MapType.Method)]
        public KGenerator Where(Function fun)
        {
            return new KGenerator
                (elements.Where(
                    e => Convert.ToBoolean(fun.Invoke(null, e))));
        }

        [MemberMap("map", MapModifier.Instance, MapType.Method)]
        public KGenerator Map(Function fun)
        {
            return new KGenerator
                (elements.Select(
                    e => fun.Invoke(null, e)));
        }

        [MemberMap("reverse", MapModifier.Instance, MapType.Method)]
        public KGenerator Reverse(Function fun)
        {
            return new KGenerator(elements.Reverse());
        }

        [MemberMap("toList", MapModifier.Instance, MapType.Method)]
        public KList ToList()
        {
            return new KList(elements);
        }

        [MemberMap("range", MapModifier.Static, MapType.Method)]
        public static KGenerator Range(int start, int end)
        {
            return new KGenerator
                (Enumerable.Range(start, end - start + 1)
                           .Select(x =>(object)(double)x));
        }

        public override KString ToStr()
        {
            return KUtil.ToString(elements);
        }

        public IEnumerator<object> GetEnumerator()
        {
            yield return elements;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}
