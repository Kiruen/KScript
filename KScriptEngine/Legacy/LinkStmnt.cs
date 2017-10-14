using KScript.KSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    //已弃用
    public sealed class LinkStmnt : ASTList
    {
        public ASTree Source{ get { return this[0]; } }

        public LinkStmnt(List<ASTree> list) : base(list) { }

        //除非无意为之,一般情况下不会发生循环链接的错误
        //因为动态语言基本上不需要在使用一个类的时候
        //必须知道另一个类的具体定义(比如一个静态的Tables,
        //它维护着一些Table变量,但并不需要using Table,除了
        //要自己创建Table的实例时候)
        public override object Evaluate(Environment env)
        {
            EngineInitor.Initial(env);
            env.PutInside("null", null);
            
            string path = AppDomain.CurrentDomain.BaseDirectory 
                          + Source.Evaluate(env).ToString() + ".ks";
            string code = File.ReadAllText(path);
            //var eval = new Evaluator(code);
            //eval.Evaluate(env);
            var lexer = new Lexer(code);
            lexer.ReadAll();
            while (lexer.TokenCount != 0)
            {
                ASTree ast = Evaluator.GenAST(lexer);
                if (!(ast is NullStmnt))
                {
                    object result = ast.Evaluate(env);
                    if (result is InstToken)
                        break;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return "using " + base.ToString();
        }
    }
}
