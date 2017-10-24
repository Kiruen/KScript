using KScript.AST;
using KScript.Callable;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.Reflection
{
    //反射元数据
    public class KMemberInfo : KBuiltIn
    {
        public string Name { get; protected set; }
        protected Environment OuterEnv { get; set; }

        public KMemberInfo(string fldName, Environment outer)
        {
            Name = fldName;
            OuterEnv = outer;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class KFieldInfo : KMemberInfo
    {
        public KFieldInfo(string fldName, Environment outer) 
            : base(fldName, outer)
        { }

        public object GetValue(KObject obj)
        {
            return obj.Read(Name);
        }

        public void SetValue(KObject obj, object value)
        {
            obj.Write(Name, value);
        }
    }

    public class KMethodInfo : KMemberInfo
    {
        public string Body
        {
            get
            {
                return FuncDef.Body.ToString();
            }
        }
        public string Signature
        {
            get
            {
                return Name + FuncDef.Parameters.ToString();
            }
        }
        public DefStmnt FuncDef { get; private set; }
        public int ParamsLength { get; private set; }
        public string[] ParamNames { get; private set; }

        public KMethodInfo(string fldName, DefStmnt def, Environment outer) 
            : base(fldName, outer)
        {
            FuncDef = def;
            var paramList = def.Parameters;
            ParamsLength = paramList.Length;
            ParamNames = paramList.ParamNames.ToArray();
            //var list = paramList.ToList();
            //list.Insert(0, new ASTLeaf(new IdToken(-1, "invoker")));
            //paramList = new ParameterList(list);
            //innerEnv.PutNew("invoke", new Function(paramList, null, outer));
            //paramList.
            //metaData.Add("invoke", new Function()
        }

        public object Invoke(KObject obj, IEnumerable<object> args)
        {
            var olfuncs = obj.Read<Function>(Name);
            return Arguments.Call(olfuncs, OuterEnv, args.ToArray());
            //var list = args.Select(arg => new ASTValue(arg))
            //               .Cast<ASTree>()
            //               .ToList();
            ////不能截断参数表！因为不确定是哪个重载版本
            //var _args = new Arguments(list);    //list.Take(func.ParamsLength).ToList()
            //return _args.Evaluate(OuterEnv, olfuncs);
        }
    }
}
