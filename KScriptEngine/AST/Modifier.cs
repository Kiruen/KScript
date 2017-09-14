using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class Modifier : ASTList
    {
        //public string Name
        //{
        //    get { return Token.Text; }
        //}
        public string Name{ get; set; }
        //public Modifier(Token t) : base(t) { }
        public Modifier(List<ASTree> list) : base(list)
        {
            Name = (children[0] as ASTLeaf).Text;
        }
    }
}
