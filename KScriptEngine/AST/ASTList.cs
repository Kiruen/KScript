using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class ASTList : ASTree
    {
        protected List<ASTree> children;
        public ASTList(List<ASTree> list) { children = list; }
        public override int ChildrenCount
        {
            get { return children.Count; }
        }

        public override int LineNo
        {
            get
            {
                foreach(ASTree ast in children)
                {
                    int no = ast.LineNo;
                    if (no != UNKNOW_LINE)
                        return no;
                }
                return UNKNOW_LINE;
            }
        }

        public override IEnumerable<ASTree> Children
        {
            get { return children; }
        }

        public override int LowerBound
        {
            get { return children.Where(ast => !ast.IsEmpty)
                                .Select(ast => ast.LowerBound).Min(); }
        }

        public override int UpperBound
        {
            get { return children.Where(ast => !ast.IsEmpty)
                                .Select(ast => ast.UpperBound).Max(); }
        }

        public override ASTree this[int i]
        {
            get { return children[i]; }
        }

        public override IEnumerator<ASTree> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("<");
            string sep = "";
            foreach(ASTree ast in children)
            {
                builder.Append(sep);
                sep = " ";
                builder.Append(ast.ToString());
            }
            return builder.Append('>').ToString();
        }

        /// <summary>
        /// 由AST数组创建一个ASTList,自动过滤空的ASTList
        /// </summary>
        /// <param name="asts"></param>
        /// <returns></returns>
        public static ASTList MakeList(params ASTree[] asts)
        {
            return new ASTList(asts
                .Where(ast => !(ast is ASTList) || ast is ASTList && ast.ChildrenCount != 0)
                .ToList());
        }
    }
}
