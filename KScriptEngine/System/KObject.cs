using KScript.AST;
using KScript.Callable;
using KScript.Execution;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript
{
    public class KObject
    {
        protected Environment innerEnv;        //用于容纳对象实例成员的内部环境
        protected ClassInfo classInfo;

        public KObject(Environment inEnv, ClassInfo info = null)
        {
            innerEnv = inEnv;
            //添加自定义类型对象(或类元对象)标志字段
            if (info != null || !(this is NativeObject))
            {
                classInfo = info;
                AddMember("-class", null);
            }
            //将类的内环境插入对象内环境之后
            //这里可能有隐患,请注意！
            //发现了致命的错误(因为每一条链都是单例,枉然修改某个结点可能会发生错误)！
            //现在只能通过类名或者对象对静态变量进行读写,无法做到隐式访问
            //if (info != null)
            //    (innerEnv as NestedEnv).InsertEnvLink(info.innerEnv);
        }

        //TODO:使用空对象模式,区别C#的null和Ks的null
        /// <summary>
        /// 获取成员,若不存在则产生异常
        /// </summary>
        /// <param name="member">成员名称</param>
        /// <returns></returns>
        public virtual object Read(string member)
        {
            Environment env = Where(member);
            //返回成员(字段或函数实例。可访读写静态成员)
            if(env == null)
            {
                if (classInfo != null)
                    return classInfo.Read(member);
                throw new KException("bad member access: " + member, Debugger.CurrLineNo);
            }
            else
                return env.Get(member);
            //var res = env == null ? 
            //           classInfo?.Read(member) : env.Get(member);
            //return res;
        }

        public TRes Read<TRes>(string member)
        {
            return (TRes)Read(member);
        }

        /// <summary>
        /// 试图获取成员,可能返回null.应避免此null和真null混用
        /// </summary>
        /// <param name="member">成员名称</param>
        /// <returns></returns>
        public virtual object TryRead(string member)
        {
            Environment env = Where(member);
            return env == null ? classInfo?.TryRead(member) : env.Get(member);
        }

        public TRes TryRead<TRes>(string member)
        {
            return (TRes)TryRead(member);
        }

        /// <summary>
        /// 为对象指定成员变量赋值(若不存在,是不会新建的)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        public virtual void Write(string member, object value)
        {
            //试图寻找此成员所处环境
            Environment env = Where(member);
            if (env == null)
            {
                //classInfo?.Write(member, value);
                if (classInfo != null)
                {
                    classInfo.Write(member, value);
                }
                else
                {
                    throw new KException("bad member access: " + member, Debugger.CurrLineNo);
                }
            }
            else
                env.PutInside(member, value);
        }

        /// <summary>
        /// 向对象的直接内部环境中添加新变量
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        public void AddMember(string member, object value)
        {
            innerEnv.PutInside(member, value);
        }

        protected Environment Where(string member)
        {
            //判断是否为最外层环境(即最外层的祖先类的内部环境,它与
            //MainScope直接相邻)。不可能仅给某个对象一个孤立的内部环境
            Environment env = innerEnv.Where(member);
            //判断是否是从当前环境或其父类环境(继承性环境)找到的该变量
            //-class用于标记是否为对象环境,做到访问成员时与其他环境隔离
            if (env != null && (env == innerEnv || env.Contains("-class")))         
                return env;
            else
                return null;
        }

        public virtual KString ToStr()
        {
            var func = (TryRead<Function>("_str"))?[0] as Function;
            if (func == null)
            {
                return ToString();
            }
            else
            {
                return func.Invoke(null).ToString();
            }
        }

        public override string ToString()
        {
            return "<object at " + GetHashCode() + ">";
        }
    }
}
