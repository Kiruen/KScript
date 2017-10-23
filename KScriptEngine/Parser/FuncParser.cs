using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript
{
    public class FuncParser : BasicParser
    {
        protected Parser param, parames, paramlist, def, args, postfix;

        public FuncParser()
        {
            //param = rule0().Option(modifier).AddId(reserved);
            param = rule0().Or(
                rule0().Ast(modifier).AddId(reserved), 
                rule0().AddId(reserved)
            );
            //param = rule0().AddId(reserved);
            //可变长参数表:指定某参数为params;第二个没用。。为何？
            //param = rule0().Or(rule0().Sep("params").AddId(reserved), rule0().AddId(reserved));
            //param = rule0().Option(rule0().Sep("params")).AddId(reserved);
            parames = rule(typeof(ParameterList))
                        .Ast(param)
                        .Rep(rule0().Sep(",").Ast(param));
            paramlist = rule0().Sep("(").Maybe(parames).Sep(")");

            def = rule(typeof(DefStmnt)).Sep("def").Option(modifier)
                        .AddId(reserved).Ast(paramlist).Ast(block);
            args = rule(typeof(Arguments)).Ast(expr).Rep(rule0().Sep(",").Ast(expr));
            postfix = rule0().Sep("(").Maybe(args).Sep(")");    //由(识别！

            //varName(..,..)->args->expr->factor->primary->VarName(expr1, expr2...)
            //simple(相当于primary带';')->expr->factor->primary->VarName(expr1, expr2...)
            //Modification: support function and closure
            reserved.Add(")");
            reserved.Add("func");
            primary.Rep(postfix);                           //函数调用,只不过不是语句,无需加分号(比如在函数参数括号内调用函数)
            //primary.InsertChoice(rule(typeof(ClosureFunc))
            //       .Sep("func").Ast(paramlist).Ast(block));
            //定义闭包、lambda表达式
            primary.InsertChoice(rule0().Or(
                rule(typeof(LambdaExpr)).Sep("func")
                .Ast(paramlist).Ast(block),
                rule(typeof(LambdaExpr)).Sep("$")
                .Or(paramlist, parames)
                //为了兼容lambda表达式,特此修改匹配顺序。
                //之前的顺序:expr, block。
                .Sep("=>").Or(block, expr)  
                ));
            simple.Option(args).Rep(rule0().Sep(";"));      //函数调用语句可不加括号
            program.InsertChoice(def);
        }
    }
}
