using KScript.Callable;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using KScript.KSystem.Reflection;
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

        //创建基本类型对象
        public ClassInfo(string name, Environment outer = null)
            : base(new NestedEnv(outer))
        {
            Name = name;
            DeclareEnv = outer;             //定义该类型的环境,可为空(全局性)
            ClassLoader.Load(this);         //向类加载器加载此对象
            PutCommonFields();
        }

        //在基本类型的基础上创建自定义类型对象
        public ClassInfo(ClassStmnt def, Environment outer)
            :this(def.Name, outer)
        {
            if(def.Name == "BigNum")
            {
                ;
            }
            definition = def;
            //初始化成员(执行成员的定义,隐藏内部维护的成员(以'-'开头的非用户使用字段))
            InitStaticMember(def, outer);
            Fields = innerEnv.Names
                     .Where(n => !n.StartsWith("-"))
                     .Where(n => !(innerEnv.Get(n) is Function))
                     .Select(n => new KFieldInfo(n, outer))
                     .ToArray();
            Methods = innerEnv.Names
                     .Where(n => !n.StartsWith("-"))
                     .Where(n => innerEnv.Get(n) is Function)
                     .SelectMany(n => innerEnv.Get(n) as OLFunction,
                      (n, func) => new KMethodInfo(n, func as Function, outer))
                     .ToArray();
            //完善构造函数(注意！这里只是改变Body的内容,因为此处
            //的Body和defstamnt共用一个body
            if (innerEnv.Contains(Name))
            {
                (innerEnv.Get(Name) as OLFunction)
                .Select(cons => cons.Body.InsertCode("return this;"))
                .ToArray();
                innerEnv.UpdateName(Name, "_cons");
            }

            //留下静态成员
            innerEnv.Names
                 //.OrderBy(x => x[0])  将静态成员排在前面防止同名成员被删除
                 //不再支持同名成员(指非静态和静态重名)
                 .Where(name => !name.StartsWith("-"))
                 .Select(name =>
                 {
                     if (!name.StartsWith("@"))
                         innerEnv.RemoveInside(name);
                     else
                         innerEnv.UpdateName(name, name.Substring(1));
                     return 0;
                 }).ToArray();
            //添加隐含静态成员(一定要最后添加,因为很多对象都需要先进行构建)
            innerEnv.PutInside("getMethod", new NativeFunc("GetMethodInfo", this));
            innerEnv.PutInside("fields", Fields);
            innerEnv.PutInside("methods", Methods);
            PutCommonFields();
        }

        //加载公有的类字段
        private void PutCommonFields()
        {
            innerEnv.PutInside("type", ClassLoader.GetOrCreateClass("Type"));
            innerEnv.PutInside("name", Name);
        }

        public void InitStaticMember(ClassStmnt def, Environment outer)
        {
            object obj = outer.Get(def.SuperClass);   //向外层查询父类info
            if (obj == null)
                Super = null;
            else if (obj is ClassInfo)
            {
                Super = obj as ClassInfo;
                (innerEnv as NestedEnv).InsertEnvLink(Super.innerEnv);
            }
            else
                throw new KException("unknown super class: " + def.SuperClass, def, def.LineNo);

            Body.IniForClassInfo(innerEnv);
        }

        //public bool HasStaticMember(string mbName)
        //{
        //    return innerEnv.Contains(mbName);
        //}

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
