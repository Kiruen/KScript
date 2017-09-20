using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    class ASTValue : ASTree
    {
        public object Value { get; set; }
        public ASTValue(object value)
        {
            Value = value;
        }
        public override ASTree this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<ASTree> Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int ChildrenCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int LineNo
        {
            get { return UNKNOW_LINE; }
        }

        public override int LowerBound
        {
            get { return UNKNOW_LINE; }
        }

        public override int UpperBound
        {
            get { return UNKNOW_LINE; }
        }

        public override IEnumerator<ASTree> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override object Evaluate(Environment env)
        {
            return Value;
        }
    }
}
