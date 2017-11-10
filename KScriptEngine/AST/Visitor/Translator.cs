using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class Translator : Visitor
    {
        public string Target { get; private set; }

        public override void Visit(ASTree ast)
        {
            if (ast is DefStmnt)
                Target = Translate(ast as DefStmnt);

        }

        public string Translate(ASTList ast)
        {
            return "";
        }

        public string Translate(DefStmnt ast)
        {
            var res = ast.Parameters
                        //.OfType<ASTLeaf>()
                        .Select(param =>
                        {
                            if (param[0][0].ToString() == "*")
                                return $"params object[] {param[1]}";
                            else
                                return $"object {param}";
                        });
            var paramList = res.Count() > 0 ? res.Aggregate((x, y) => x + ", " + y) : "";
            return $@"public object {ast.Name}({paramList}) 
                    {{
                        {Translate(ast.Body)}
                    }}";
        }
    }
}
