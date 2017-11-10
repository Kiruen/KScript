using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("Range", MapModifier.Static, MapType.CommonClass)]
    public class KRange : KBuiltIn, IEnumerable<object>
    {
        public string LeftSym { get; }
        public string RightSym { get; }
        public double LeftBound { get; }
        public double RightBound { get; }

        [MemberMap("_cons", MapModifier.Static, MapType.Constructor)]
        public KRange(KString ls, double lb, double rb, KString rs)
        {
            LeftSym = ls;
            RightSym = rs;
            LeftBound = lb;
            RightBound = rb;
        }

        public bool Contains(double val)
        {
            bool left = LeftSym == "[" ? LeftBound <= val : LeftBound < val,
                right = RightSym == "]" ? val <= RightBound : val < RightBound;
            return left && right;
        }

        public override KString ToStr()
        {
            return base.ToStr();
        }

        public override string ToString()
        {
            return "range: " + LeftSym + LeftBound + "," + RightBound + RightSym;
        }

        public IEnumerator<object> GetEnumerator()
        {
            double start = LeftSym == "[" ? LeftBound : LeftBound + 1;
            double end = RightSym == "]" ? RightBound : RightBound - 1;
            for (double n = start; n <= end; n++)
            {
                yield return n;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
