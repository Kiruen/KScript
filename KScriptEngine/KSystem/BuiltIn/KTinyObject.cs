using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("TinyObj", MapModifier.Static, MapType.CommonClass)]
    public class KTinyObject : KBuiltIn
    {
        [MemberMap("_cons", MapModifier.Instance, MapType.Constructor, true)]
        public KTinyObject(params object[] memberNames)
        {
            foreach (var name in memberNames)
                AddMember(name.ToString(), null);
        }
    }
}
