using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript
{
    public class ClassParser : FuncParser
    {
        Parser member, classBody, classDef;

        public ClassParser()
        {
            member = rule0().Or(def, simple);
            classBody = rule(typeof(ClassBody)).Sep("{").Rep(member).Sep("}");
            classDef = rule(typeof(ClassStmnt)).Sep("class").AddId(reserved)
                .Option(rule0().Sep(":").AddId(reserved))
                .Ast(classBody);

            postfix.InsertChoice(rule(typeof(Dot)).Sep(".").AddId(reserved));
            program.InsertChoice(classDef);

            reserved.Add("class");
            reserved.Add(":");
        }
    }
}
