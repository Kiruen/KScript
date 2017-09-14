using KScript.AST;
using KScript.Callable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KSystem.Reflection
{
    //反射元数据
    public class KMemberInfo
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
                return Method.Body.ToString();
            }
        }
        public string Signature
        {
            get
            {
                return Name + Method.Parameters.ToString();
            }
        }
        public Function Method { get; private set; }
        public int ParamsLength { get; private set; }
        public string[] ParamNames { get; private set; }

        public KMethodInfo(string fldName, Function func, Environment outer) 
            : base(fldName, outer)
        {
            Method = func;
            var paramList = func.Parameters;
            ParamsLength = func.Parameters.Length;
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
            var list = args.Select(arg => new ASTValue(arg))
                           .Cast<ASTree>()
                           .ToList();
            var func = obj.Read(Name) as Function;
            //截断参数表
            var _args = new Arguments(list);    //list.Take(func.ParamsLength).ToList()
            return _args.Evaluate(OuterEnv, func);
            //var func = (obj.Read(Name) as Function)[ParamsLength];
            //var env = func.CreateNewEnv();
            //for(int i = 0; i < ParamsLength; i++)
            //{
            //    func.Parameters.Evaluate(env, i, args.ElementAt(i));
            //}
            //func.Body.Evaluate(env);
        }
    }
}
