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
        /// 用于创建类元数据时调用的初始化程序(普通、静态成员会一并初始化)
        /// 会将所有成员添加到env中,以供进一步分类、处理
        /// 注意！静态变量的名称仍然带有@
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public void IniForClassInfo(Environment env)
        {
            foreach (var member in this)
                member.Evaluate(env);
        }

        public void InsertDef(DefStmnt def)
        {
            throw new NotImplementedException("NotImplemented!");
        }
    }
}
