using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class VariableCollector : Visitor
    {
        public IDictionary<string, object> temp;

        public VariableCollector(IDictionary<string, object> temp)
        {
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
            else if (ast is ClassStmnt)
                Collect(ast as ClassStmnt);
            //抽象分支放到最后！
            else if (ast is ASTList)
                Collect(ast as ASTList);
        }

        public void Collect(VarName varName)
        {
            if (!temp.ContainsKey(varName.Name))
                temp.Add(varName.Name, null);
        }

        public void Collect(ASTList list)
        {
            foreach (var child in list.Children)
                child.Accept(this);
        }


        public void Collect(DeclareExpr decl)
        {
            string varName, prefix = decl.Modifier == null ? "" : decl.Modifier.Name;
            foreach (var ast in decl.Children)
            {
                varName = prefix + (ast[0] as ASTLeaf).Text;
                if (!temp.ContainsKey(varName))
                    temp.Add(varName, null);
            }
        }

        public void Collect(DefStmnt def)
        {
            var totalName = def.Modifier?.Name + def.Name;
            if (!temp.ContainsKey(totalName))
                temp.Add(totalName, null);
        }

        public void Collect(ClassStmnt clstmnt)
        {
            if (!temp.ContainsKey(clstmnt.Name))
                temp.Add(clstmnt.Name, null);
        }
    }
}
