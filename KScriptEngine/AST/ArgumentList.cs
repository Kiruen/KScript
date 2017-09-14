using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ArgumentList : ASTList
    {
        public ArgumentList(List<ASTree> list) : base(list) { }
        public int Length
        { get { return ChildrenCount; } }
    }
}
