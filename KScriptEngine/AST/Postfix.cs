using KScript.Callable;
using KScript.Runtime;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public abstract class Postfix : ASTList
    {
        public Postfix(List<ASTree> list) : base(list) { }
        public abstract object Evaluate(Environment env, object prefix);
    }

    /// <summary>
    /// 函数实参表后缀
    /// </summary>
    public class Arguments : Postfix
    {
        public int Length { get { return ChildrenCount; } }

        public Arguments(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment callerEnv, object prefix)
        {
            if (prefix is IFunction)
            {
                return (prefix as IFunction)[Length].Invoke(callerEnv, this);
            }
            else
                throw new KException("Not a function object", this, LineNo);
        }

        /// <summary>
        /// 动态调用某个(组中指定的)函数实例
        /// </summary>
        /// <param name="func">函数实例</param>
        /// <param name="callerEnv">调用者的环境</param>
        /// <param name="args">参数表</param>
        /// <returns></returns>
        public static object Call(IFunction func, Environment callerEnv, params object[] args)
        {
            int len = args?.Length ?? 0;
            return func[len].Invoke(callerEnv, args);
        }
    }

    public class Dot : Postfix
    {
        public Dot(List<ASTree> list) : base(list) { }

        //.后面跟的标识符
        public string Name
        {
            get { return (children[0] as ASTLeaf).Text; }
        }

        public override object Evaluate(Environment env, object prefix)
        {
            string member = Name;
            //引用类
            if (prefix is ClassInfo)
            {
                ClassInfo info = prefix as ClassInfo;
                if ("new" == member)
                {
                    //创建实例的内部环境
                    NestedEnv newEnv = new NestedEnv(info.DeclareEnv);   //创建对象内部环境
                    //判断是内建类型还是自定义类型
                    KObject kobj = info.IsBuiltIn ? null : IniObject(info, newEnv);
                    //试图获得构造函数
                    var constr = (info.IsBuiltIn ? info : kobj)?.TryRead("_cons"); //as Function; //info.Name
                    if (constr != null)
                    {
                        //从环境中删除构造器,以防重复调用
                        newEnv.RemoveInside("_cons"); //info.Name
                        //返回构造器,交给下一级调用
                        return constr;
                    }
                    //默认构造函数,直接返回对象(可能为null(当内建类无法实例化时))
                    else return kobj;
                }
                //引用静态成员
                else
                    return info.Read(member);
            }
            //引用对象的成员
            else if (prefix is KObject)
            {
                return (prefix as KObject).Read(member);  //返回成员字段或成员函数实例
            }
            //临时包装一个未知对象(包括通过类名进行反射操作的行为)
            return NativeObject.Pack(prefix, env).Read(member);
            //else
            //    throw new KException("bad member access: " + member, this);
        }

        /// <summary>
        /// 初始化对象,即执行对象的类定义里的内容(不等于构造函数！相当于C++的类内初始化)
        /// </summary>
        /// <param name="info">类的元数据</param>
        /// <param name="finalEnv">最终构建的类的内环境</param>
        /// <param name="currentEnv">当前递归层次(某个祖先类)正在使用的内环境</param>
        /// <returns></returns>
        protected KObject IniObject(ClassInfo info, NestedEnv innerEnv)
        {
            KObject kobj = new KObject(innerEnv, info);
            innerEnv.PutInside("this", kobj);                //this指针
            innerEnv.PutInside("type", info);                //类元数据,用于反射
            if (info.Super != null)
            {
                var superInEnv = new NestedEnv();
                //插入直接父类的内部环境
                innerEnv.InsertEnv(superInEnv);
                var super = IniObject(info.Super, superInEnv);
                //上面两句位置一定不能对调！因为父类初始化时
                //需要用到外部环境(如全局环境)中的对象,如果
                //对调,则由于superInEnv是孤立的而导致初始化失败
                innerEnv.PutInside("super", super);          //仿照js的prototype模式
            }
            info.Body.Evaluate(innerEnv);                    //会向当前环境里添加变量、函数实例等
            //向类定义中附加一份内部构造函数(初始化用),以供子类调用
            //注意,此处的_init相当于不带返回值的_cons,先于_cons被构造
            var initor = new OLFunction();
            innerEnv.PutInside("_init", initor);
            if (innerEnv.Contains(info.Name))
            {
                //将原先构造器的所有重载版本放入新的init对象中
                innerEnv.Get<OLFunction>(info.Name)
                        .Select(cons => initor.Add(cons.Clone() as Function))
                        .ToArray();
                //改变构造器名称(防止跟类名重复)
                innerEnv.UpdateName(info.Name, "_cons");
            }
            return kobj;
        }

        public override string ToString()
        {
            return "." + Name;
        }
    }

    public class ArrayRef : Postfix
    {
        public ASTree Index
        {
            get { return children[0]; }
        }

        public ArrayRef(List<ASTree> list) : base(list) { }

        public override object Evaluate(Environment env, object prefix)
        {
            try
            {
                //索引值可以是任意类型的
                object index = Index.Evaluate(env);
                if (prefix is object[])
                    return (prefix as object[])[Convert.ToInt32(index)];
                else if (prefix is List<object>)
                {
                    return (prefix as List<object>)[Convert.ToInt32(index)];
                }
                //调用索引器
                else if (prefix is KObject)
                {
                    //var args = new Arguments(new List<ASTree>() { new NumberLiteral(new NumToken(-1, index)) });
                    //return args.Evaluate(env, (prefix as KObject).Read("getter"));
                    //首先处理基本类型
                    var getter = prefix.GetType().GetMethod("get_Item");
                    if (getter != null)
                        return getter.Invoke(prefix, new[] { index });
                    else
                        return Arguments.Call((prefix as KObject).Read<IFunction>("getter"), env, index);
                }
                //提供.net原生序列对象的读取操作
                else
                {
                    dynamic collection = prefix;
                    return Enumerable.ElementAt(collection, Convert.ToInt32(index));
                }
            }
            catch(Exception exc)
            {
                throw new KException($"bad array access:\r\n{exc.Message}", LineNo);
            }
        }

        public override string ToString()
        {
            return "[" + Index + "]";
        }
    }
}
