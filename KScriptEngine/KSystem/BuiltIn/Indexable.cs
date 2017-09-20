using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    public interface Indexable //<in TIndex>
    {
        object this[object index] { get; set; }
    }
}
