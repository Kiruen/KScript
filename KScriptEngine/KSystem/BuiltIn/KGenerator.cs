using KScript.Callable;
using KScript.KAttribute;
using KScript.Runtime;
using KScript.Utils;
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
        public KGenerator Where(IFunction fun)
        {
            if (fun.IsOLFuncSet)
                fun = fun[2] ?? fun[1] ??
                    throw new KException("No matched function!", Debugger.CurrLineNo);
            if (fun.ParamsLength == 2)
                return new KGenerator
                    (elements.Where
                    ((e, x) => Convert.ToBoolean(fun.Invoke(null, e, (double)x))));
            else
                return new KGenerator
                    (elements.Where
                    (e => Convert.ToBoolean(fun.Invoke(null, e))));
        }

        [MemberMap("map", MapModifier.Instance, MapType.Method)]
        public KGenerator Map(IFunction fun)
        {
            if (fun.IsOLFuncSet)
                fun = fun[2] ?? fun[1] ??
                    throw new KException("No matched function!", Debugger.CurrLineNo);
            if (fun.ParamsLength == 2)
                return new KGenerator
                        (elements.Select
                        ((e, i) => fun.Invoke(null, e, (double)i)));
            else
                return new KGenerator
                        (elements.Select
                        (e => fun.Invoke(null, e)));
        }

        [MemberMap("spread", MapModifier.Instance, MapType.Method)]
        public KGenerator Spread(IFunction setSelector)
        {
            return new KGenerator
                    (elements.SelectMany
                    (e => setSelector.Invoke(null, e) as IEnumerable<object>));
        }

        [MemberMap("reverse", MapModifier.Instance, MapType.Method)]
        public KGenerator Reverse()
        {
            return new KGenerator(elements.Reverse());
        }

        [MemberMap("zip", MapModifier.Instance, MapType.Method)]
        public KGenerator Zip(IEnumerable<object> second, IFunction selector)
        {
            return new KGenerator(elements.Zip(second, (x, y)=> selector.Invoke(null, x, y)));
        }

        [MemberMap("partial", MapModifier.Instance, MapType.Method)]
        public KList Partial(IFunction keySelector)
        {
            return new KList(elements
                .GroupBy(e => keySelector.Invoke(null, e))
                .Select(g => new KList(g.ToList())));
        }

        [MemberMap("groupBy", MapModifier.Instance, MapType.Method)]
        public KDict Group(IFunction keySelector)
        {
            return new KDict(elements.GroupBy(e => keySelector.Invoke(null, e))
                .ToDictionary(g => g.Key, g => new KList(g.ToList()) as object));
        }

        [MemberMap("groupBy", MapModifier.Instance, MapType.Method)]
        public KDict Group(IFunction keySelector, IFunction valSelector)
        {
            return new KDict(elements.GroupBy
                    ((e => keySelector.Invoke(null, e)),
                     (e => valSelector.Invoke(null, e)))
                    .ToDictionary(g => g.Key, g => new KList(g.ToList()) as object));
        }

        [MemberMap("collect", MapModifier.Instance, MapType.Method)]
        public object Collect(IFunction selector)
        {
            return elements.Aggregate((x, y) => selector.Invoke(null, x, y));
        }

        [MemberMap("max", MapModifier.Instance, MapType.Method)]
        public object Max(IFunction selector)
        {
            return elements.Max(x => selector.Invoke(null, x));
        }

        [MemberMap("min", MapModifier.Instance, MapType.Method)]
        public object Min(IFunction selector)
        {
            return elements.Min(x => selector.Invoke(null, x));
        }

        [MemberMap("sum", MapModifier.Instance, MapType.Method)]
        public double Sum(IFunction selector)
        {
            return elements.Sum(x => Convert.ToDouble(selector.Invoke(null, x)));
        }

        [MemberMap("sum", MapModifier.Instance, MapType.Method)]
        public double Sum()
        {
            return elements.Select(x => Convert.ToDouble(x)).Sum();
        }

        [MemberMap("count", MapModifier.Instance, MapType.Method)]
        public double Count(IFunction selector)
        {
            return (double)elements.Count(x => Convert.ToBoolean(selector.Invoke(null, x)));
        }

        [MemberMap("count", MapModifier.Instance, MapType.Method)]
        public double Count()
        {
            return (double)elements.Count();
        }

        [MemberMap("skip", MapModifier.Instance, MapType.Method)]
        public KGenerator Skip(int count)
        {
            return new KGenerator(elements.Skip(count));
        }

        [MemberMap("take", MapModifier.Instance, MapType.Method)]
        public KGenerator Take(int count)
        {
            return new KGenerator(elements.Take(count));
        }

        [MemberMap("toList", MapModifier.Instance, MapType.Method)]
        public KList ToList()
        {
            return new KList(elements);
        }

        [MemberMap("toSet", MapModifier.Instance, MapType.Method)]
        public KSet ToSet()
        {
            return new KSet(elements);
        }

        [MemberMap("toDict", MapModifier.Instance, MapType.Method)]
        public KDict ToDict(IFunction keySelector, IFunction valSelector)
        {
            return new KDict(elements.ToDictionary
                ((e => keySelector.Invoke(null, e)),
                 (e => valSelector.Invoke(null, e))));
        }

        [MemberMap("forEach", MapModifier.Instance, MapType.Method)]
        public void ForEach(IFunction action)
        {
            if (action.ParamsLength == 2)
            {
                int indexer = 0;
                foreach (var val in elements)
                    action.Invoke(null, val, indexer++);
            }
            else
            {
                foreach (var val in elements)
                    action.Invoke(null, val);
            }
        }

        [MemberMap("range", MapModifier.Static, MapType.Method)]
        public static KGenerator Range(int start, int end)
        {
            return new KGenerator
                (Enumerable.Range(start, end - start + 1)
                           .Select(x =>(object)(double)x));
        }

        [MemberMap("random", MapModifier.Static, MapType.Method)]
        public static KGenerator Random(int start, int end, int count)
        {
            Random ran = new Random();
            return new KGenerator
                (Enumerable.Range(0, count)
                           .Select(x => (object)(double)ran.Next(start, end)));
        }

        [MemberMap("random", MapModifier.Static, MapType.Method)]
        public static KGenerator Random(int count)
        {
            Random ran = new Random();
            return new KGenerator
                (Enumerable.Range(0, count)
                           .Select(x => (object)ran.NextDouble()));
        }

        public override KString ToStr()
        {
            return KUtil.ToString(elements);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}