using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript   //.Parser 类名与命名空间重名！
{
    public class ArrayParser : ClassParser
    {
        Parser elements, kvpair, kvpairs;

        public ArrayParser()
        {
            reserved.Add("]");
            //reserved.Add(">");
            //注意！elements和新的postfix没有关系哦！
            //elements = rule(typeof(ArrayLiteral)).Ast(expr)
            //                .Rep(rule0().Sep(",").Ast(expr));
            var list = rule0().Sep("[").Maybe(TokenList(typeof(ListLiteral), expr)).Sep("]");
            var tuple = rule0().Sep("tup").Sep("(").Maybe(TokenList(typeof(TupleLiteral), expr)).Sep(")");

            var dict = rule0()/*.Sep("dict")*/.Sep("{").Maybe(TokenList(typeof(DictLiteral), expr)).Sep("}");
            //var set = rule0().Sep("set").Sep("{").Maybe(TokenList(typeof(SetLiteral), expr)).Sep("}");


            //kvpair = rule(typeof(Pair)).Ast(expr).Sep(":").Ast(expr);
            //kvpairs = rule0().Ast(kvpair).Rep(rule0().Sep(",").Ast(kvpair));
            //var dict = rule(typeof(Dict)).Sep("dict").Sep("{")
            //                  .Maybe(kvpairs).Sep("}");

            //var dict = rule(typeof(Dict)).Sep("dict").Sep("{")
            //                  .Rep(rule0().Ast(expr).Sep(",")).Sep("}");


            primary.InsertChoice(dict);
            //primary.InsertChoice(set);
            primary.InsertChoice(list);
            primary.InsertChoice(tuple);
            postfix.InsertChoice(rule(typeof(ArrayRef))
                          .Sep("[").Ast(expr).Sep("]"));
        }
    }
}
