using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    public class KInStream : KBuiltIn
    {
        StreamReader stream;
        public KInStream(KString path)
        {
            stream = new StreamReader(path);
        }
    }

    public class KOutStream : KBuiltIn
    {

    }
}
