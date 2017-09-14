using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class NullStmnt : ASTList
    {
        public NullStmnt(List<ASTree> c) : base(c) { }
    }
}
