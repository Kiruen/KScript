using KScript.AST;
using KScript.Callable;
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

        public virtual object Read(string member)
        {
            Environment env = Where(member);
            //返回成员(字段或函数实例。可访读写静态成员)
            var res = env == null ? 
                       classInfo?.Read(member) : env.Get(member);
            return res;
        }

        /// <summary>
        /// 为对象指定成员变量赋值(若不存在,是不会新建的)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        public virtual void Write(string member, object value)
        {
            Environment env = Where(member);
            if (env == null)
                classInfo?.Write(member, value);
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
            if (env != null && (env == innerEnv || env.Contains("-class")))         
                return env;
            else
                return null;
                //throw new KException("member: " + member + " not found", this);
        }

        public virtual KString ToStr()
        {
            var func = (Read("_str") as Function)?[0];
            if (func == null) return ToString();
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
