using KScript.AST;
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
    /// <summary>
    /// 内建类基类
    /// </summary>
    public abstract class KBuiltIn : KObject
    {
        //储存反复用到的函数反射对象
        //设置为双层字典,因为每个BuiltIn类的派生类都要有一个自己的字典
        private static Dictionary<string, Dictionary<string, MemberInfo>> globalCache
                       = new Dictionary<string, Dictionary<string, MemberInfo>>(16);

        //不使用静态构造函数,因为无法保证执行顺序(可能需要等到第一次实例化才执行)
        //加载好需要用到的原生函数的Info
        //static KBuiltIn()
        //{
            //foreach (var type in
            //    Assembly.GetExecutingAssembly()
            //            .GetTypes()
            //            .Where(t => t.Namespace == "KScript.KSystem.BuiltIn")
            //            //是派生类
            //            .Where(t => t != typeof(KBuiltIn)))
            //{
            //    var attr = type.GetCustomAttribute<MemberMapAttribute>();
            //    var typeName = attr != null ? attr.MappingName : type.Name;
            //    var cache = new Dictionary<string, MemberInfo>(16);
            //    globalCache.Add(typeName, cache);
            //    ClassInfo classInfo = ClassLoader.GetOrCreateClass(typeName);
            //    KUtil.FindMapping(type,
            //        (name, memInfo) => cache.Add(name, memInfo));
            //    //添加静态成员和构造函数
            //    KUtil.AddNatMemberFromMapping(cache, (name, info) =>
            //    {
            //        if (info is ConstructorInfo)
            //            classInfo.AddMember(name, NativeMember.Create
            //                                       (name, info, null));
            //    });
            //}
        //}

        /// <summary>
        /// 创建一个内建类基类对象
        /// </summary>
        /// <param name="env">对象外部的闭包环境,可空</param>
        public KBuiltIn(Environment env = null) 
            : base(new NestedEnv(env))
        {
            var typeAttr = GetType().GetCustomAttribute<MemberMapAttribute>();
            //var typeName = typeAttr != null ? typeAttr.MappingName : type.Name;
            //只有添加特性的类才会被当成内置类处理,否则只是
            //单纯地继承了内置类(而不希望被当成BuiltIn)
            if (typeAttr != null)
            {
                var typeName = typeAttr.MappingName;
                var cache = globalCache[typeName];
                //添加普通成员
                KUtil.AddNatMemberFromMapping(cache, (name, attr, info) =>
                {
                    if (!(info is ConstructorInfo || attr.Modifier == MapModifier.Static))
                        innerEnv.PutInside(name, NativeMember.Create
                                                    (name, info, this));
                });
                //添加固有成员
                this.AddMember("type", ClassLoader.GetOrCreateClass(typeName));
                //this.AddMember("this", this);
            }
        }

        public override object Read(string member)
        {
            var res = base.Read(member);
            if (res is NativeData)
                res = (res as NativeData).GetValue();
            return res;
        }

        public override void Write(string member, object value)
        {
            var old = base.Read(member);
            if (old is NativeData)
                (old as NativeData).SetValue(value);
            else
                base.Write(member, value);
        }

        /// <summary>
        /// 初始化一个原生/内建类型(若已加载此类型,则直接返回info对象)
        /// </summary>
        /// <param name="type"></param>
        public static ClassInfo IniBuitInClass(string typeName, Type type)
        {
            //创建类信息对象
            var classInfo = ClassLoader.GetOrCreateClass(typeName);
            if (!globalCache.ContainsKey(typeName))
            {
                var cache = new Dictionary<string, MemberInfo>(16);
                globalCache.Add(typeName, cache);
                //注意,这里可能会因为误用重载特性而重复添加成员,待解决
                //查找映射成员,并添加到缓存中
                KUtil.FindMapping(type,
                    (name, memInfo) => cache.Add(name, memInfo));
                //添加静态成员和构造函数
                KUtil.AddNatMemberFromMapping(cache, (name, attr, info) =>
                {
                    if (info is ConstructorInfo || attr.Modifier == MapModifier.Static)
                        classInfo.AddMember(name, NativeMember.Create
                                                   (name, info, null));
                });
            }
            return classInfo;
            //    throw new KException(string.Format("Invalid native class: {0}!", type.Name), null);
        }

        /// <summary>
        /// 初始化所有基础的引擎内建类型
        /// </summary>
        public static void IniBuitInClasses()
        {
            if (globalCache.Count != 0) return;

            foreach (var type in
                    Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(t => t.Namespace == "KScript.KSystem.BuiltIn")
                            //筛除隐藏类(编译期间系统附加的类型)
                            .Where(t => t.IsVisible)
                            //是派生类(而不是KBuiltIn这个基类)
                            .Where(t => t != typeof(KBuiltIn)))
            {
                var attr = type.GetCustomAttribute<MemberMapAttribute>();
                if(attr != null)
                {
                    IniBuitInClass(attr.MappingName, type);
                }
            }
        }
    }
}
