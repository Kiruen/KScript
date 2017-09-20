using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class VariableCollector : Visitor
    {
        private IDictionary<string, object> temp;
        private int currLine;

        public VariableCollector(IDictionary<string, object> temp, int currLine)
        {
            this.currLine = currLine;
            this.temp = temp;
        }

        public override void Visit(ASTree ast)
        {
            if (ast is VarName)
                Collect(ast as VarName);
            else if (ast is DeclareExpr)
                Collect(ast as DeclareExpr);
            else if (ast is DefStmnt)
                Collect(ast as DefStmnt);
            else if (ast is Closure)
                Collect(ast as Closure);
            else if (ast is ClassStmnt)
                Collect(ast as ClassStmnt);
            else if (ast is BlockStmnt)
                Collect(ast as BlockStmnt);
            //抽象分支放到最后！
            else if (ast is ASTList)
                Collect(ast as ASTList);
        }

        //public void Collect(ASTLeaf leaf)
        //{
        //    if (Regex.Match(@"[_a-zA-Z][_a-zA-Z\d]*", leaf.Text) != null &&
        //        TestName(leaf.Text))
        //        temp.Add(leaf.Text, null);
        //}

        public void Collect(BlockStmnt block)
        {
            if (TestRange(block))
                Collect(block as ASTList);
        }

        private void Collect(ParameterList plist)
        {
            foreach (ASTree aid in plist)
            {
                string id = ParameterList.ParamName(aid);
                if (TestName(id))
                    temp.Add(id, null);
            }
        }

        public void Collect(VarName varName)
        {
            if (TestName(varName.Name))
                temp.Add(varName.Name, null);
        }

        public void Collect(ASTList list)
        {
            foreach (var child in list.Children)
                Visit(child);
        }

        public void Collect(DeclareExpr decl)
        {
            string varName; //, prefix = decl.Modifier == null ? "" : decl.Modifier.Name;
            foreach (var ast in decl.Children)
            {
                varName = (ast[0] as ASTLeaf).Text;
                if (TestName(varName))
                    temp.Add(varName, null);
            }
        }

        public void Collect(DefStmnt def)
        {
            var totalName = /*def.Modifier?.Name + */def.Name;
            if (TestName(totalName))
                temp.Add(totalName, null);
            if (TestRange(def))
            {
                Collect(def.Parameters);
                Collect(def.Body);
            }
        }

        public void Collect(Closure close)
        {
            if (TestRange(close))
            {
                Collect(close.Parameters);
                Collect(close.Body);
            }
        }

        public void Collect(ClassStmnt clstmnt)
        {
            if(TestName(clstmnt.Name))
                temp.Add(clstmnt.Name, null);
            if (TestRange(clstmnt))
                Collect(clstmnt.Body);
        }

        private bool TestName(string varName)
        {
            return !temp.ContainsKey(varName);
        }

        private bool TestRange(ASTree ast)
        {
            return ast.LowerBound <= currLine &&
                        ast.UpperBound >= currLine;
        }
    }
}
