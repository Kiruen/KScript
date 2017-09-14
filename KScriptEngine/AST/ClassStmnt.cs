using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ClassStmnt : ASTList
    {
        public ClassStmnt(List<ASTree> list) : base(list) { }

        public string Name
        {
            get { return (children[0] as ASTLeaf).Text; }
        }

        //最多只允许继承一个类;第二个应该是表示父类的Id
        public string SuperClass
        {
            get
            {
                return ChildrenCount < 3 ? null : (children[1] as ASTLeaf).Text;
            }
        }

        public ClassBody Body
        {
            get { return (ClassBody)children[ChildrenCount - 1]; }
        }

        public override object Evaluate(Environment env)
        {
            ClassInfo info = new ClassInfo(this, env);
            env.Put(Name, info);
            return Name;
        }

        public override string ToString()
        {
            string parent = SuperClass;
            if (parent == null) parent = "*";
            return "<class " + Name + " extends " + parent + Body + ">";
        }
    }
}
