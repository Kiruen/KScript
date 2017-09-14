using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public abstract class Visitor
    {
        public abstract void Visit(ASTree ast);
    }
}
