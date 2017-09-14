using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("NameSpace", MapModifier.Static, MapType.CommonClass)]
    public class KNameSpace : KBuiltIn
    {
        private static HashSet<string> specialVars
            = new HashSet<string>()
            {
                "_add", "_sub", "_mul", "_div",
                "_dmul", "_str", "_init", "_cons",
                "-class", "main", "setter", "getter",
                "iterator"
            };

        private Environment realEnv { get; set; }
        public string Name { get; protected set; }

        //此处具有极大隐患:原生模块和用户自定义模块
        //创建的命名空间对象所使用的真实环境有所不同
        //(用户自定义模块会另外建立一层变量环境,而原生
        //模块则直接使用外面一层的环境(出于效率))
        [MemberMap("variables", MapModifier.Instance, MapType.Data)]
        public string[] Variables
        {
            get { return realEnv.Names.Union(innerEnv.Names).ToArray(); }
        }

        public KNameSpace(Environment env, string name) 
            : base(env)
        {
            realEnv = env;
            Name = name;
            //innerEnv.PutInside("type", ClassLoader.GetClass("Namespace"));
        }

        public override object Read(string member)
        {
            Environment env = Where(member);
            //可访问到外部变量
            return env == null ? innerEnv.Get(member) : base.Read(member);
        }

        public void DumpInto(Environment env, bool _override = false)
        {
            foreach (var name in Variables.Except(specialVars)) //new HashSet<object>(Variables).Except
                DumpInto(name, env, _override);
        }

        public void DumpInto(string varName, Environment env, bool _override = false)
        {
            bool exist = env.Contains(varName);
            if (Where(varName) != null && (!exist || exist && _override))
                env.PutInside(varName, Read(varName));
        }

        public override KString ToStr()
        {
            return string.Format("<Namespace: {0}>", Name);
        }
    }
}
