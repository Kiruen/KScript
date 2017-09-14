using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class ASTLeaf : ASTree
    {
        private static List<ASTree> Empty = new List<ASTree>();

        protected Token Token { get; private set; }
        public string Text
        {
            get { return Token.Text; }
        }

        public override int ChildrenCount
        {
            get { return 0; }
        }

        public override int LineNo
        {
            get  { return Token.LineNo; }
        }

        public override ASTree this[int i]
        {
            get { return this; }
        }

        public ASTLeaf(Token token) { Token = token; }

        //返回普通叶子的值(即一个代表名称的字符串)
        public override object Evaluate(Environment env)
        {
            return Token.Text;
        }

        public override IEnumerable<ASTree> Children
        {
            get { return Empty; }
        }

        public override IEnumerator<ASTree> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Token.Text;
        }
    }
}
