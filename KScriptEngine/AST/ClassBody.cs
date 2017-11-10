using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ClassBody : ASTList
    {
        public ClassBody(List<ASTree> list) : base(list) { }

        /// <summary>
        /// 用于创建对象时调用的初始化程序(不会初始化静态成员)
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public override object Evaluate(Environment env)
        {
            foreach (var member in this)
            {
                //跳过静态成员的声明
                if (member is DeclareExpr && (member as DeclareExpr).IsStatic
                 || member is DefStmnt && (member as DefStmnt).IsStatic)
                    continue;
                else
                    member.Evaluate(env);       //若添加成员和环境中的变量重名该怎么办
            }
            return null;
        }

        /// <summary>
        /// 用于创建类元数据时调用的初始化程序
        /// 会将所有静态成员添加到env中,以供进一步分类、处理
        /// 非静态成员将打包返回
        /// 注意！静态变量的名称将不再带有@
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public List<ASTree> InitForClassInfo(Environment env)
        {
            //foreach (var member in this)
            //    member.Evaluate(env);
            List<ASTree> nonStaticTemp = new List<ASTree>(16);
            foreach (var ast in this)
            {
                if(ast is DefStmnt) 
                {
                    var def = ast as DefStmnt;
                    if (def.IsStatic/* || def.Name == className*/)
                        def.Evaluate(env);
                    else
                        nonStaticTemp.Add(def);
                }
                else if (ast is DeclareExpr)
                {
                    var decl = ast as DeclareExpr;
                    if (decl.IsStatic)
                        decl.Evaluate(env);
                    else
                        nonStaticTemp.Add(decl);
                }
                //TODO:using语句对于静态和非静态作用域都有效
                else if (ast is InstExpr && (ast as InstExpr).InstName == "using")
                {
                    ast.Evaluate(env);
                }
                else
                {
                    throw new KException("Invalid expression in class body!", ast.LineNo);
                }
            }
            return nonStaticTemp;
        }

        public void InsertDef(DefStmnt def)
        {
            throw new NotImplementedException("NotImplemented!");
        }
    }
}
