using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KScript.KAttribute
{
    public enum MapType : byte
    {
        Data = 0,
        Method = 1,
        Constructor = 2,
        CommonClass = 3,
        ToolClass = 4
    }

    public enum MapModifier : byte
    {
        Static = 0,
        Instance = 1
    }
    /// <summary>
    /// 原生类型成员映射特性,用于建立原生成员与KS对象的映射
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
                    AllowMultiple = true,
                    Inherited = true)]
    public class MemberMapAttribute : Attribute
    {
        public string MappingName { get; private set; }
        public MapModifier Modifier { get; private set; }
        public MapType MapType { get; private set; }

        public MemberMapAttribute(string mappingName, MapModifier mod, MapType mapType)
        {
            MappingName = mappingName;
            Modifier = mod;
            MapType = mapType;
        }
    }
}
