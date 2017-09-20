using KScript.Execution;
using KScript.KAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KScript.Callable
{
    public abstract class NativeMember
    {
        protected object invoker;
        public string Name { get; protected set; }
        public bool IsStatic { get; protected set; }

        public NativeMember() { }

        public NativeMember(string name, object invoker)
        {
            this.invoker = invoker;
            IsStatic = invoker == null;
            Name = name;
        }

        public static NativeMember Create(MemberMapAttribute attr, MemberInfo info, object invoker = null)
        {
            var m_type = info.MemberType;
            switch (m_type)
            {
                case MemberTypes.Constructor: case MemberTypes.Method:
                    {
                        return new NativeFunc(attr.MappingName, info as MethodBase, attr.IsVarParams, invoker);
                    }
                case MemberTypes.Property: case MemberTypes.Field:
                    return new NativeData(attr.MappingName, info, invoker);
                default: throw new KException("Can't create caller with error type", Debugger.CurrLineNo);
            }
        }
    }

    public class NativeData : NativeMember
    {
        private MemberInfo member;

        public NativeData(string name, MemberInfo info, object invoker = null)
            : base(name, invoker)
        {
            member = info;
        }

        public object GetValue()
        {
            return (member as dynamic).GetValue(invoker);
        }

        public void SetValue(object val)
        {
            (member as dynamic).SetValue(invoker, val);
        }
    }
}
