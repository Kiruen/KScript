using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    /// <summary>
    /// 分支语法
    /// </summary>
    public class MatchStmnt : ASTList
    {
        public MatchStmnt(List<ASTree> c) : base(c) { }
        public ASTree Sample { get { return this[0]; } }
        public ASTree DefaultBlock
        {
            get { return this[ChildrenCount - 1]; }
        }

        public override object Evaluate(Environment env)
        {
            ASTree ast;
            object sample = Sample.Evaluate(env);
            if (sample == null) return null;

            for (int i = 1; i < ChildrenCount; i++)
            {
                ast = children[i];
                //非default语句(一个代码块可以对应多个标签)
                if (ast.ChildrenCount >= 2)
                {
                    int last = ast.ChildrenCount - 1;
                    var block = ast[last];
                    foreach (var expr in ast.Take(last))
                    {
                        var mcase = expr.Evaluate(env);
                        //自动分析、匹配范围
                        if ((sample is double && mcase is KRange && 
                            (mcase as KRange).Contains((double)sample))
                           || sample.Equals(mcase))
                        {
                            return block.Evaluate(env);
                        }
                        else if (mcase is KRegex && (mcase as KRegex).Test(sample.ToString()))
                        {
                            return block.Evaluate(env);
                        }
                    }
                }
                //default语句
                else if (ast.ChildrenCount == 1)
                {
                   return ast[0].Evaluate(env);
                }
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < ChildrenCount; i++)
            {
                if (children[i].ChildrenCount == 2)
                    builder.Append(" when <").Append(children[i][0] + ">").Append(children[i][1]);
                else
                    builder.Append(" default ").Append(children[i]);
            }
            return "<match " + Sample + builder.Append(">").ToString();
        }
    }
}
