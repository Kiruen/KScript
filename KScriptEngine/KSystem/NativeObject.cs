
using KScript.Callable;
using KScript.Execution;
using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KScript.AST
{
    /// <summary>
    /// 对原生对象进行包装,使其能够通过反射访问成员。使用了装饰者(代理?)模式
    /// </summary>
    public class NativeObject : KObject
    {
        //全局原生对象
        public static Dictionary<string, NativeObject> globalObjs
                      = new Dictionary<string, NativeObject>(16);
        //反射用到的标记
        public static readonly BindingFlags BINDNG_FLAG_INSTANCE = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        public static readonly BindingFlags BINDNG_FLAG_STATIC = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static;
        //原型对象
        public object protoObj = null;

        public NativeObject(object proto, Environment env, ClassInfo info = null)
            : base(env, info)
        {
            protoObj = proto;
            //env.PutNew("type", proto.GetType().Name);
        }

        /*
        当试图读取未加载的成员函数时,自动包装成一个
        Native Function(此处无需调用,留给postfix调用)
        */
        public override object Read(string member)
        {
            //读取原生函数实例
            object result = base.TryRead(member);
            if (result != null)
                return result;
            //若不是自定义变量,则读取原生的属性或方法
            PropertyInfo proInfo;   //用于获取属性值
            object invoker = null;
            if (protoObj is Type)
            {
                proInfo = (protoObj as Type).GetProperty(member, BINDNG_FLAG_STATIC);
            }
            else
            {
                //Type type = prototype.GetType();
                //if (type.IsGenericType)
                //    type = type.GetGenericTypeDefinition()
                //           .MakeGenericType(type.GetGenericArguments()); 
                //type.GetGenericTypeDefinition() => typeof(Class<,,>);
                invoker = protoObj;
                proInfo = protoObj.GetType().GetProperty(member, BINDNG_FLAG_INSTANCE);
            }
            //将延迟构造一个临时的函数实例
            if (proInfo == null)
                return new NativeFunc(member, protoObj, protoObj is Type);
            //存在这个属性,则返回属性值
            else
                result = proInfo.GetValue(invoker, null);
            //结果的类型转换
            if (result is int)
                result = Convert.ToDouble(result);
            else if (result is bool)
                result = ASTree.BOOL[(bool)result];
            return result;
        }

        public override void Write(string member, object value)
        {
            //if (innerEnv.Contains(member))
            //{
            //    base.Write(member, value);
            //    return;
            //}
            //若不是自定义变量,则写入原生对象的属性
            PropertyInfo proInfo;
            if (protoObj is Type)
            {
                proInfo = (protoObj as Type).GetProperty(member, BINDNG_FLAG_STATIC);
            }
            else
            { 
                proInfo = protoObj.GetType().GetProperty(member, BINDNG_FLAG_INSTANCE);
            }
            //TODO:使任意属性都可以得到正确类型的值
            if (proInfo.PropertyType == typeof(int))
                value = Convert.ToInt32(value);
            else if (proInfo.PropertyType == typeof(bool))
                value = Convert.ToBoolean(value);
            else if (proInfo.PropertyType.IsEnum)
                value = Enum.Parse(proInfo.PropertyType, value.ToString(), true);
            //值为NativeObject,则取其装饰的原对象
            else if (value is NativeObject)
                value = (value as NativeObject).protoObj;
            //赋值
            proInfo.SetValue(protoObj, value, null);
        }

        //public NativeObject AddNativeFunc(string funcName, string nativeName, params Type[] parames)
        //{
        //    try
        //    {
        //        Type type;
        //        //以原对象为调用者
        //        object invoker = protoObj;
        //        //判断是单例(静态类)还是实例
        //        bool isSingleton = protoObj is Type;
        //        if (isSingleton)
        //        {
        //            type = (Type)protoObj;
        //            invoker = null;
        //        }
        //        else
        //        {
        //            type = protoObj.GetType();
        //        }
        //        MethodInfo method = type.GetMethod(nativeName, parames);
        //        innerEnv.PutInside(funcName, NativeMember.Create(funcName, method, invoker));
        //        return this;
        //    }
        //    catch
        //    {
        //        throw new KException("Cannot find a native function: " + nativeName, Debugger.CurrLineNo);
        //    }
        //}

        /// <summary>
        /// 建立附带特性的方法的映射(需要自行为方法设置FuncMap特性,且方法必须是公有的)
        /// </summary>
        public NativeObject CreateFuncMap()
        {
            var type = protoObj is Type ? protoObj as Type : protoObj.GetType();
            KUtil.FindMapping(type, 
            (m_attr, methodInfo) =>
            {
                innerEnv.PutInside(m_attr.MappingName, NativeMember.Create
                (m_attr, methodInfo, protoObj is Type ? null : protoObj));
            });
            return this;
        }

        /// <summary>
        /// 以匿名的形式临时包装一个系统内的对象(作为局部变量,不会添加到环境中)
        /// </summary>
        public static NativeObject Pack(object proto, Environment outer)
        {
            Environment innerEnv = new NestedEnv(outer);
            return new NativeObject(proto, innerEnv);
        }
    }
}
