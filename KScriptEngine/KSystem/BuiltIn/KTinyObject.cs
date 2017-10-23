using KScript.Callable;
using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.BuiltIn
{
    [MemberMap("TinyObj", MapModifier.Static, MapType.CommonClass)]
    public class KTinyObject : KBuiltIn
    {
        protected KString TypeName { get; set; }

        [MemberMap("members", MapModifier.Instance, MapType.Data)]
        public object[] Members { get; private set; } 

        [MemberMap("_cons", MapModifier.Instance, MapType.Constructor, true)]
        public KTinyObject(KString typeName, params object[] memberNames)
        {
            TypeName = typeName;
            Write("type", ClassLoader.GetOrCreateClass(typeName));
            Members = memberNames;
            foreach (var name in memberNames)
            {
                AddMember(name.ToString(), null);
            }
        }

        public void Initial(params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                this.Write(this.Members[i].ToString(), values[i]);
            }
        }

        //利用原型模式创建对象,节省开销,避免冲突
        private static KTinyObject prototype = null;
        [MemberMap("create", MapModifier.Static, MapType.Method, true)]
        public static NativeFunc CreateFactory(KString typeName, params object[] memberNames)
        {
            prototype = new KTinyObject(typeName, memberNames);
            return new NativeFunc("instance", typeof(KTinyObject).GetMethod("Instance", BindingFlags.NonPublic | BindingFlags.Static), true);
        }

        private static KTinyObject Instance(params object[] values)
        {
            var instance = prototype.Clone();
            instance.Initial(values);
            return instance;
        }

        private KTinyObject Clone()
        {
            return new KTinyObject(TypeName, Members);
        }
    }
}
