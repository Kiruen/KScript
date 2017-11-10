using KScript.Callable;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using KScript.KSystem.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace KScript.AST
{
    public class ClassInfo : KObject
    {
        protected ClassStmnt definition;
        //创建类对象的环境
        public Environment DeclareEnv { get; private set; }
        public string Name { get; set; } /*{ return definition.Name; }*/

        /// <summary>
        /// 表明类型是否为内建类
        /// </summary>
        public bool IsBuiltIn
        {
            get { return definition == null; }
        }

        public ClassBody Body
        {
            get { return definition?.Body; }
        }

        public ClassInfo Super { get; private set; }
        public KFieldInfo[] Fields { get; private set; }
        public KMethodInfo[] Methods { get; private set; }

        /// <summary>
        /// 创建基本类型对象
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="outer">定义类型的环境</param>
        /// <param name="willComplete">指明在构造函数执行完毕后此Class是否属于完整的Class</param>
        public ClassInfo(string name, Environment outer = null, bool willComplete = true)
            : base(new NestedEnv(outer))
        {
            Name = name;
            DeclareEnv = outer;             //定义该类型的环境,可为空(全局性)
            ClassLoader.Load(this);         //向类加载器加载此对象
            if(willComplete)
                PutCommonFields();
        }

        //设计极不合理！亟待优化
        //在基本类型的基础上创建自定义类型对象
        public ClassInfo(ClassStmnt classDef, Environment outer)
            :this(classDef.Name, outer, false)
        {
            definition = classDef;
            //初始化成员(执行静态成员的定义,返回非静态成员的AST
            //注意,对于declar语句,不会拆分其中的若干表达式)
            List<ASTree> nonStaticTemp = InitMembers(classDef, outer);
            //整合各种反射信息
            Fields = nonStaticTemp
                     .OfType<DeclareExpr>()
                     .SelectMany(decl => decl.Children)
                     .Select(expr => (expr[0] as ASTLeaf).Text)
                     .Select(name => new KFieldInfo(name, outer))
                     .ToArray();
            Methods = nonStaticTemp
                     .OfType<DefStmnt>()
                     .Select(_def => new KMethodInfo(_def.Name, _def, outer))
                     .ToArray();
            //完善构造函数(注意！这里只是改变Body的内容,
            //因为此处的Body和defStamnt共用一个Body对象)
            nonStaticTemp.OfType<DefStmnt>()
                         .Where(func => func.Name == this.Name)
                         .Select(consDef => consDef.Body.InsertCode("return this;"))
                         .ToArray();
            //if (innerEnv.Contains(Name))
            //{
            //    innerEnv.Get<OLFunction>(Name)
            //    .Select(cons => cons.Body.InsertCode("return this;"))
            //    .ToArray();
            //    innerEnv.UpdateName(Name, "_cons");
            //}
            //添加隐含静态成员(一定要最后添加,因为很多对象都需要先进行构建)
            innerEnv.PutInside("getMethod", new NativeFunc("GetMethodInfo", this));
            innerEnv.PutInside("fields", Fields);
            innerEnv.PutInside("methods", Methods);
            //添加共有字段(重复添加,因为上述步骤会把先前添加的变量删除)
            PutCommonFields();
        }

        //加载ClassInfo公有的字段
        private void PutCommonFields()
        {
            innerEnv.PutInside("type", ClassLoader.GetOrCreateClass("Type"));
            innerEnv.PutInside("name", Name);
        }

        /// <summary>
        /// 初始化静态成员; 
        /// 返回一个非静态成员集合以供构造元数据; 
        /// 修改构造函数的名称和部分定义
        /// </summary>
        /// <param name="def"></param>
        /// <param name="outer"></param>
        /// <returns></returns>
        public List<ASTree> InitMembers(ClassStmnt def, Environment outer)
        {
            object obj = outer.Get(def.SuperClass);   //向外层查询父类info
            if (obj == null)
                Super = null;
            //将父类ClassInfo的内部环境链接到本Info内部环境的外部(实现静态成员的继承访问)
            else if (obj is ClassInfo)
            {
                Super = obj as ClassInfo;
                (innerEnv as NestedEnv).InsertEnvLink(Super.innerEnv);
            }
            else
                throw new KException("unknown super class: " + def.SuperClass, def, def.LineNo);
            //执行类体中的所有语句
            return Body.InitForClassInfo(innerEnv);
        }

        public KMethodInfo GetMethodInfo(KString methodName)
        {
            return Methods.Where(m => m.Name == methodName.ToString()).ElementAt(0);
        }

        public override string ToString()
        {
            return "<class " + Name + ">";
        }
    }
}
