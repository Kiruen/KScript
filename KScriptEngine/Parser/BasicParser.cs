using KScript.AST;
using System;
using System.Collections.Generic;
using Operators = KScript.Parser.Operators;

namespace KScript
{
    /// <summary>
    /// 基于组合子的可配置简易语法分析器(体现了初等的函数式编程思维和流畅接口思想)
    /// </summary>
    public class BasicParser
    {
        protected delegate Parser Rule0();
        protected delegate Parser Rule(Type nodeType);

        public readonly static HashSet<string> reserved = new HashSet<string>()
            {
                "}", Token.EOL, "if", "else", "other", "for", "foreach",
                "in", "match", "when", "while", "@", "*",
                "try", "catch", "default", "extends", "class", "var", "def",
                "tup", /*"dict", "set"*/
            };

        public readonly static string[]
            instruction = new string[]
            {
                "return", "continue", "break", "new",
                "using", "throw", "assert", "del"
            },
            modifiers = new string[]
            {
                "@", "*"
            },
            monads = new string[]
            {
                "@", "-", "!", "~", "&", "not"
            };

        public readonly static int ASSIGN_OP = -5;
        public readonly static Operators operators = new Operators()
        {
            { "<<=",ASSIGN_OP,false}, { ">>=",ASSIGN_OP,false}, {"^=",ASSIGN_OP,false},
            { "|=",ASSIGN_OP,false}, { "&=",ASSIGN_OP,false}, {"**=",ASSIGN_OP,false},
            { "+=",ASSIGN_OP,false}, { "-=",ASSIGN_OP,false}, {"*=",ASSIGN_OP,false},
            { "/=",ASSIGN_OP,false}, {"%=",ASSIGN_OP,false}, {"=",ASSIGN_OP,false},
            { "||",1,true}, {"&&", 2,true},{ "or",1,true}, {"and", 2,true},
            { "?", 1, true }, { ":", 2, true },
            { "in",2,true }, { "is",2,true },
            { "...", 3, true }, { "~", 3, true },
            {"^",4,true}, {"&",4,true}, {"|",4,true}, {">>",4,true}, {"<<",4,true},
            {"==",4,true}, {"!=",4,true},
            { ">",4,true}, {">=",4,true}, {"<",4,true}, {"<=",4,true},
            { "∩",4,true }, { "∪",4,true }, { "∈",4,true }, { "⊆",4,true }, { "⊂",4,true },
            {"+",5,true}, { "-",5,true},
            {"*",6,true}, {"/",6,true}, {"%",6,true},
            {"**",7,true},
        };
        protected Parser primary, declaration, expr, commaExpr, factor,  //因子,进行乘除运算的数  
               block, simple, modifier, statement, program;  //, main;

        //用于简化代码的委托实例
        protected Rule0 rule0 = Parser.Rule;
        protected Rule rule = Parser.Rule;  //带参数的重载版本

        public BasicParser()
        {
            /*2017年3月9日: 千万不要把Sep放在开头,除非以这个
              分隔符为开头的语法是唯一的！像换行符就不行！*/

            //expr0、statement0都是为了递归定义更加清晰。你先在body里面放一个空parser
            //,即stat0,然后定义真正的statment时使用body,这样stat0里就有了body,
            //而body里也有stat0
            Parser expr0 = rule0();

            //定义基本元素
            primary = rule(typeof(PrimaryExpr))
                    .Or(rule0().Sep("(").Ast(expr0).Sep(")") //加括号的表达式,直接返回表达式的AST,无需Shift
                   //, rule(typeof(MallocStmnt)).Sep("new").Sep("Array")
                   // .Rep(rule0().Sep("[").Ast(expr0).Sep("]"))
                   , rule(typeof(InstExpr)).AddToken("using").AddStr(typeof(StringLiteral)).Maybe(rule0().Sep(":").Ast(TokenList(null, rule0().AddId(reserved))))
                   , rule(typeof(InstExpr)).AddToken(instruction).Maybe(expr0)
                   , rule0().AddNum(typeof(NumberLiteral))
                   , rule0().AddId(typeof(VarName), reserved)
                   , rule0().Or(
                         rule0().AddStr(typeof(StringLiteral)),
                         rule0().Sep("%").AddStr(typeof(ExprStringLiteral)),
                         rule0().Sep("/").AddStr(typeof(RegexLiteral))
                     )
                   , rule(typeof(RangeExpr)).Sep("#").AddToken("[", "(")
                    .Maybe(expr0).Sep(",").Maybe(expr0).AddToken("]", ")")
                   );

            //定义表达式的项
            factor = rule0().Or(rule(typeof(MonadExpr)).Rep(rule0().AddToken(monads)).Ast(primary) //单目运算
                                , primary);

            //↓不太好理解 subExp的作用
            //定义表达式
            expr = expr0.Expression(typeof(BinaryExpr), factor, operators);
            /*expr0.InsertChoice(rule0().Or(rule(typeof(Range)).Sep("range").Ast(factor).Sep("between")
              .Ast(factor).Sep("and").Ast(factor));*/

            //逗号表达式
            commaExpr = rule(typeof(CommaExpr)).Ast(expr)
                       .Rep(rule0().Sep(",").Ast(expr));

            Parser statement0 = rule0();
            //"1"是数字,不会被sep或addToken检测到
            //定义由一对大括号包含的程序体
            block = rule(typeof(BlockStmnt))
                    .Sep("{")
                    .Rep(rule0().Ast(statement0))
                    .Sep("}");

            //修饰符(怎样才能区分是变量的修饰符还是函数的修饰符？？？)
            modifier = rule(typeof(Modifier)).AddToken(modifiers);

            //定义变量声明语句(向当前作用域中添加变量,不会使用外层的同名变量)
            var declaration0 = rule0().AddId(reserved).Maybe(rule0().Sep("=").Ast(expr));
            declaration = rule(typeof(DeclareExpr))
                    .Sep("var").Option(modifier)
                    .Ast(declaration0)
                    .Rep(rule0().Sep(",").Ast(declaration0));

            //简单的语句只有放在前面,才能保证不被复杂语句抢先分析
            //定义一条简单语句或空语句(带分号)
            simple = rule0().Or(declaration,
                        rule(typeof(PrimaryExpr)).Ast(expr),
                        rule0().Rep(rule0().Sep(";")));


            //定义程序段,可以是可嵌套的含程序体的语法，也可以是单条语句
            statement = statement0.Or(
                      //rule(typeof(LinkStmnt)).Sep("using").AddStr(typeof(StringLiteral))
                    //这里的if then作为ifstmnt的孩子，不会像rule0()那样创建一棵树
                    //而是两棵树(一个expr树和一个block树)
                    rule(typeof(IfStmnt)).Sep("if").Ast(expr).Ast(block)
                     //关于换行标记
                     //首先必须明确:换行标记是必要的！否则很多语法都会受到制约；
                     //其次,这里是在if和else之间加了Rep来跳过多个换行标记
                     //(Rep必须Match到了才循环Parse,否则就会跳过Rep,很有用哦)
                     //Rep更像是正则表达式的.*: 匹配某模式0次或多次
                     //Option则是 be? :匹配0次或1次  //.Rep(rule0().Sep(";", Token.EOL))
                     .Rep(rule0().Sep("else").Sep("if").Ast(expr).Ast(block))
                     .Option(rule0().Sep("other").Ast(block))
                    , rule(typeof(WhileStmnt)).Sep("while").Ast(expr).Ast(block)
                    , rule(typeof(MatchStmnt)).Sep("match").Ast(expr)
                     .Rep(rule0().Rep(rule0().Sep("when").Ast(expr)).Ast(block))
                     .Option(rule0().Sep("default").Ast(block))
                    , rule(typeof(ForStmnt)).Sep("for").Sep("(")
                     .Maybe(rule0().Or(declaration, commaExpr)).Sep(";")
                     .Maybe(expr).Sep(";").Maybe(commaExpr).Sep(")").Ast(block)
                    , rule(typeof(ForeachStmnt)).Sep("foreach").Sep("(")
                     .AddId(reserved).Sep("in").Ast(expr)
                     .Sep(")").Ast(block)
                    , rule(typeof(ExceptionStmnt)).Sep("try")
                     .Ast(block).Sep("catch").Maybe(rule0().Sep("(")
                     .AddId(reserved).Sep(")")).Ast(block)
                    , simple);

            program = rule0().Or(statement, rule(typeof(NullStmnt)))
                       .Rep(rule0().Sep(";", Token.EOL));
            //main = rule0().Sep("program").Sep("{")
            //                        .Ast(program).Sep("}");
            //程序实际上被切分成很多program(单条语
            //句也算)所以不能用main概括program
        }

        public Parser TokenList(Type rootType, Parser tokenType)
        {
            Parser root = (rootType == null ? rule0() : rule(rootType));
            return root.Ast(tokenType).Rep(rule0().Sep(",").Ast(tokenType));
        }

        public ASTree Parse(Lexer lexer)
        {
            return program.Parse(lexer);
        }
    }
}
