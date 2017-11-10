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
        //Parser elements, kvpair, kvpairs;

        public ArrayParser()
        {
            reserved.Add("]");
            //之前碰到了老问题：Maybe里面如果生成了一个匿名的ASTList,
            //是会进行优化的(即假如里面是一条AST,则不用一个List盛装此
            //AST,而是将此AST当成ASTList(如果是的话))
            //然后就是这个问题:`a, b[1:-1]` = `b[0], []`出现错误
            //(随后发现只要是ElementList形式的都有这个BUG)
            //原因是后面的[]被当成空ASTList而不是一个具体的常量,
            //于是在Reapet的Parse那儿被忽略了,直接导致没有被解析成AST！阔怕！
            //反思一下,这个优化究竟有何作用。。？
            var list = rule(typeof(ListLiteral)).Sep("[").Maybe(ElementList(expr, typeof(InitalList))).Sep("]");
            var tuple = rule(typeof(LValueTuple)).Sep("`").Maybe(ElementList(expr, typeof(InitalList))).Sep("`");
            //var list = rule0().Sep("[").Maybe(ElementList(expr, typeof(ListLiteral))).Sep("]");
            //var tuple = rule0().Sep("`").Maybe(ElementList(expr, typeof(LValueTuple))).Sep("`");
            var dict = rule(typeof(SetOrDictLiteral)).Sep("{").Maybe(ElementList(expr, typeof(InitalList))).Sep("}");

            primary.InsertChoice(dict);
            primary.InsertChoice(list);
            primary.InsertChoice(tuple);
            postfix.InsertChoice(rule(typeof(ArrayRef))
                          .Sep("[").Ast(expr).Sep("]"));
        }
    }
}
