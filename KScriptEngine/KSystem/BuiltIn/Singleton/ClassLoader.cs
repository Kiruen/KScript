using KScript.AST;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;
using System.Collections.Generic;
using System.Linq;

namespace KScript.KSystem
{
    /// <summary>
    /// 类加载器,管理各种类型,实现动态获得类信息的特性(forName)
    /// </summary>
    public static class ClassLoader
    {
        private static Dictionary<string, ClassInfo> classes 
                        = new Dictionary<string, ClassInfo>(32);
        //注意！！实例化classinfo时会自动加载类对象到字典里！
        static ClassLoader() { }

        public static void Load(ClassInfo info)
        {
            if (!classes.ContainsKey(info.Name))
            {
                classes.Add(info.Name, info);
            }
        }

        public static void Clear()
        {
            var keys = classes.Keys.ToArray();
            foreach (var name in keys)
                if (!classes[name].IsBuiltIn)
                    classes.Remove(name);
        }

        [MemberMap("forName", MapModifier.Static, MapType.Method)]
        public static ClassInfo GetClass(KString className)
        {
            return GetClass(className.ToString());
        }

        //仅供内部使用
        public static ClassInfo GetClass(string className)
        {
            ClassInfo type = null;
            classes.TryGetValue(className, out type);
            return type;
        }

        /// <summary>
        /// 试图获得类信息对象;若不存在此类,则简单构建一个临时的类信息对象
        /// </summary>
        /// <param name="className">类名</param>
        /// <returns></returns>
        public static ClassInfo GetOrCreateClass(string className)
        {
            var classInfo = GetClass(className);
            if(classInfo == null)
                classInfo = new ClassInfo(className);
            return classInfo;
        }

        /// <summary>
        /// 将常驻内存中的类信息对象放入主环境中
        /// </summary>
        /// <param name="mainEnv"></param>
        public static void DumpInto(Environment mainEnv)
        {
            foreach (var c in classes)
                mainEnv.PutInside(c.Key, c.Value);
        }
    }
}
