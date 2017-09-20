using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class IfStmnt : ASTList
    {
        public IfStmnt(List<ASTree> c) : base(c) { }
        public ASTree Condition { get { return this[0]; } }
        public ASTree ThenBlock { get { return this[1]; } }
        /*else ifs can't express*/
        public ASTree OtherBlock
        {
            get
            {
                int last = ChildrenCount - 1;
                return children[last] is BlockStmnt ? this[last] : null;
            }
        }

        public override object Evaluate(Environment ev)
        {
            object conRes = Condition.Evaluate(ev);
            if (conRes is double && (double)conRes != 0)
                return ThenBlock.Evaluate(ev);
            else if (ChildrenCount > 2)
            {
                int i = 2;
                ASTree ast = null;
                do
                {
                    ast = children[i];
                    //如何判断是else if还是other?
                    //只要判断一下这个语法树是单纯的block还是由
                    //Ast()创建的普通ASTlist(含有expr和block)即可
                    if (ast.ChildrenCount == 2 && !(ast is BlockStmnt))
                    {
                        conRes = ast[0].Evaluate(ev);
                        if (conRes is double && (double)conRes != 0)
                            return ast[1].Evaluate(ev);
                    }
                    else
                        return OtherBlock.Evaluate(ev);
                }
                while (++i < ChildrenCount);
            }
            return null;
            //else
            //    return ElseBlock == null ? 0 : ElseBlock.Evaluate(ev);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 2; i < ChildrenCount; i++)
            {
                if(children[i] is BlockStmnt)
                    builder.Append(" else ").Append(children[i]);
                else
                    builder.Append(" else if ").Append(children[i]);
            } 
            return "<if " + Condition + " " + ThenBlock + builder.ToString() +">";
        }
    }
}