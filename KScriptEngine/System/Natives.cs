using System;
using System.Reflection;
using KScript.AST;
using KScript.Callable;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;
using KScript.Execution;

namespace KScript.KSystem
{
    public static class EngineInitor
    {
        /// <summary>
        /// 加载全局性的程序模块(如全局对象、全局函数)
        /// 之所以不设置为静态构造函数是因为有些对象需要先被构造
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static void Initial(Environment env)
        {
            //LoadNativeFuncs(env);
            LoadNativeObjs(env);
            //加载基础API
            var apinsp = new KNameSpace(new NestedEnv(), "API");
            LoadNativeTypes(apinsp, new[] { typeof(API) });
            apinsp.DumpInto(env);
        }

        public static void LoadNativeObjs(Environment env)
        {
            //加载原生类型
            env.PutInside("Type", ClassLoader.GetOrCreateClass("Type"));
            //env.PutInside("Namespace", ClassLoader.GetOrCreateClass("Namespace"));
            //env.PutInside("Str", ClassLoader.GetOrCreateClass("Str"));
            //env.PutInside("Dict", ClassLoader.GetOrCreateClass("Dict"));
            env.PutInside("Arr", ClassLoader.GetOrCreateClass("Arr"));
            env.PutInside("Num", ClassLoader.GetOrCreateClass("Num"));
            env.PutInside("None", ClassLoader.GetOrCreateClass("None"));
            KBuiltIn.IniBuitInClasses();
            ClassLoader.DumpInto(env);
            //初始化所有基础内建类型
            //加载特殊对象
            env.PutInside("null", null);
            //加载原生对象(单例)
            var math = AddNativeObj(env, "Math", typeof(Math));
            math.AddMember("pi", Math.PI);
            math.AddMember("e", Math.E);

            AddNativeObj(env, "Regex", typeof(RegexProxy))
            .CreateFuncMap();
            AddNativeObj(env, "Class", typeof(ClassLoader))
            .CreateFuncMap();
            AddNativeObj(env, "File", typeof(FileProxy))
            .CreateFuncMap();
        }

        //public static void LoadNativeFuncs(Environment env)
        //{
            ////基础函数
            //LoadNativeFunc(env, "type", typeof(API), "TypeOf", typeof(object));
            //LoadNativeFunc(env, "alert", typeof(API), "Alert", typeof(object));
            //LoadNativeFunc(env, "sleep", typeof(API), "Sleep", typeof(double));
            //LoadNativeFunc(env, "writeLine", typeof(API), "WriteLine", typeof(object));
            //LoadNativeFunc(env, "println", typeof(API), "WriteLine", typeof(object));
            //LoadNativeFunc(env, "write", typeof(API), "Write", typeof(object));
            //LoadNativeFunc(env, "clear", typeof(API), "Clear");
            //LoadNativeFunc(env, "read", typeof(API), "Read");
            //LoadNativeFunc(env, "readAsyn", typeof(API), "ReadAsyn");
            //LoadNativeFunc(env, "readLine", typeof(API), "ReadLine");
            ////实用工具函数
            //LoadNativeFunc(env, "eval", typeof(API), "Eval", typeof(KString));
            //LoadNativeFunc(env, "currentTime", typeof(API), "CurrentTime");
            //LoadNativeFunc(env, "random", typeof(API), "Random", typeof(double), typeof(double));
            //LoadNativeFunc(env, "toNumber", typeof(API), "ToNumber", typeof(object));
            //LoadNativeFunc(env, "toNum", typeof(API), "ToNumber", typeof(object));
            //LoadNativeFunc(env, "toBig", typeof(API), "ToBig", typeof(object));
            //LoadNativeFunc(env, "toStr", typeof(API), "ToStr", typeof(object));
            //LoadNativeFunc(env, "dict", typeof(API), "Dict");
            //流程控制函数
            //AddNativeFunc(env, "continue", typeof(API), "Continue");
            //AddNativeFunc(env, "break", typeof(API), "Break");
            ////AddNativeFunct(env, "superbreak", typeof(Natives), "SuperBreak");
            //AddNativeFunc(env, "return", typeof(API), "Return", typeof(object));
        //}

        /// <summary>
        /// 加载原生模块(符合KS标准的.Net程序集)
        /// </summary>
        /// <param name="env"></param>
        /// <param name="modulePath"></param>
        public static void LoadNativeModule(KNameSpace knamespace, string modulePath)
        {
            //Assembly assm = Assembly.LoadFrom(modulePath);
            Assembly asm = Assembly.LoadFrom(modulePath);
            //string moduleName = assm.GetName().Name;
            LoadNativeTypes(knamespace, asm.GetTypes());
        }

        public static void LoadNativeTypes(KNameSpace knamespace, Type[] types)
        {
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<MemberMapAttribute>();
                if (attr == null) continue;
                //松散型模块(通常把所有数据放在一个工具类中)
                if (attr.MapType == MapType.ToolClass) //attr == null || 
                {
                    KUtil.FindMapping(type,
                      (name, memdInfo) =>
                      {
                          var natMember = NativeMember.Create(name, memdInfo);
                              //env.PutInside(name, natMember);
                              //向命名空间对象中添加该成员
                              knamespace?.AddMember(name, natMember);
                      });
                }
                //结构型模块(有自定义的内建类)
                else
                {
                    var classInfo = KBuiltIn.IniBuitInClass(attr.MappingName, type);
                    knamespace?.AddMember(classInfo.Name, classInfo);
                }
            }
        }

        public static void LoadNativeFunc(Environment env, string funcName, Type ownerType,
                                           string nativeName, params Type[] parames)
        {
            MethodInfo method = null;
            try
            {
                method = ownerType.GetMethod(nativeName, parames);
                env.PutInside(funcName, NativeMember.Create(funcName, method));
            }
            catch
            {
                throw new KException("Cannot find a native function: " + nativeName, Debugger.CurrLineNo);
            }
        }

        public static NativeObject AddNativeObj(Environment env, string objName, object proto)
        {
            NativeObject nobj = NativeObject.Pack(proto, env);
            env.PutInside(objName, nobj);
            return nobj;
        }
    }
}
