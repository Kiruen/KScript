using KScript.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class BlockStmnt : ASTList, ICloneable
    {
        public BlockStmnt(List<ASTree> c) : base(c) { }
        public override object Evaluate(Environment env)
        {
            object result = null;   //0
            //临时作用域
            Environment temporary = new NestedEnv(env);
            foreach (var fragment in this)
            {
                //此处很可能是另一个代码块,比如if块
                //如果if块外面包裹着一个代码块,则
                //无需在条件表达式前做暂停,此处就可以实现
                //暂停,类似的while、match也可以解决
                //至于for,foreach 要想像VS的调试功能一样
                //还是得在条件表达式前暂停
                Debugger.UpdateData(fragment.LineNo, temporary);
                Debugger.TrySuspend();
                if (!(fragment is NullStmnt))
                {
                    result = fragment.Evaluate(temporary);
                    if (result is SpecialToken)
                    {
                        //只要是阻断当前操作的特殊令牌,就返回给上层
                        //一般都是从代码块中原封不动地返回令牌
                        //然后由调用代码块的地方来检测、处理令牌
                        SpecialToken token = result as SpecialToken;
                        if (token.Text == "continue" || 
                            token.Text == "break" || 
                            token.Text == "return")
                            return token;
                    }
                }
            }
            //由null修改为了result
            return result;
        }

        private static Lexer lexer;
        /// <summary>
        /// 向程序块内插入一条语句
        /// </summary>
        /// <param name="lineNo">程序块内部行号</param>
        /// <param name="code">需要插入的代码</param>
        public bool InsertCode(int lineNo, string code)
        {
            if (lexer == null || lexer.Script != code)
                lexer = new Lexer(code);

            lexer.ReadAll();
            var res = Evaluator.Parse(lexer);
            children.Insert(lineNo, res);
            return true;
        }

        /// <summary>
        /// 向程序块末尾内插入一条语句
        /// </summary>
        /// <param name="code">需要插入的代码</param>
        public bool InsertCode(string code)
        {
            return InsertCode(ChildrenCount, code);
        }

        /// <summary>
        /// 拷贝代码块的定义
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new BlockStmnt(children);
        }
    }
}
