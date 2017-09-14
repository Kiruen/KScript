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
        Parser member, class_body, classdef;

        public ClassParser()
        {
            member = rule0().Or(def, simple);
            class_body = rule(typeof(ClassBody)).Sep("{").Rep(member).Sep("}");
            classdef = rule(typeof(ClassStmnt)).Sep("class").AddId(reserved)
                .Option(rule0().Sep("extends").AddId(reserved))
                .Ast(class_body);

            postfix.InsertChoice(rule(typeof(Dot)).Sep(".").AddId(reserved));
            program.InsertChoice(classdef);

            reserved.Add("class");
            reserved.Add("extends");
        }
    }
}
