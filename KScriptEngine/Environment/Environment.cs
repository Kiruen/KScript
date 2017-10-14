using KScript.Runtime;
using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KScript
{
    public interface Environment
    {
        object this[string varName] { get; set; }
        string[] Names { get; }

        object Get(string varName);
        T Get<T>(string varName);
        void Put(string varName, object value);
        void PutInside(string name, object value);
        bool Contains(string varName);
        bool Remove(string name);
        bool RemoveInside(string name);
        Environment Where(string name);
        void UpdateName(string oldName, string newName);
        void CopyFrom(IDictionary<string, object> table);
        void CopyFrom(object obj);
    }

    public class NestedEnv : Environment
    {
        protected Dictionary<string, object> variables 
                    = new Dictionary<string, object>(8);
        protected Environment outerEnv;

        public string[] Names
        {
            get
            {
                return variables.Keys.ToArray();
            }
        }

        public object this[string varName]
        {
            get
            {
                return variables[varName];
            }
            set
            {
                variables[varName] = varName;
                //throw new NotImplementedException("未实现环境setter");
            }
        }

        //public CancellationToken CancelToken { get; set; }

        public NestedEnv(Environment outer = null)
        {
            outerEnv = outer;
        }

        public object Get(string varName)
        {
            if (varName == null) return null;
            object value = null;
            //被修改过,可能有潜在隐患
            //variables.TryGetValue(varName, out value);
            if (Contains(varName))
                value = variables[varName];
            else if (outerEnv != null)  //value == null && outerEnv != null
                value = outerEnv.Get(varName);
            else
                throw new KException("Variable: '" + varName + "' is not defined", Debugger.CurrLineNo);
            return value;
        }

        public T Get<T>(string varName)
        {
            return (T)Get(varName);
        }

        public void Put(string varName, object value)
        {
            Environment env = Where(varName);
            if (env == null)
                env = this;
            env.PutInside(varName, value);
        }

        public bool Contains(string varName)
        {
            return variables.ContainsKey(varName);
        }

        public void PutInside(string varName, object value)
        {
            if (!Contains(varName))
                variables.Add(varName, value);
            else
                variables[varName] = value;
        }

        /// <summary>
        /// 优先在内层环境中查找
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public Environment Where(string varName)
        {
            if (Contains(varName))
                return this;
            else if (outerEnv == null)
                return null;
            else
                return ((NestedEnv)outerEnv).Where(varName);
        }

        public bool Remove(string name)
        {
            throw new NotImplementedException();
        }

        public bool RemoveInside(string name)
        {
            if (variables.ContainsKey(name))
                return variables.Remove(name);
            return false;
        }

        /// <summary>
        /// 向当前环境和其外部环境中间插入一个中介环境(相当于单链表的插入)
        /// (注意,插入的环境可能会换掉其直接外部环境)
        /// </summary>
        /// <param name="env">中介环境</param>
        public void InsertEnv(NestedEnv env)
        {
            if (env == null || env == this) return;
            Environment temp = outerEnv;
            outerEnv = env;
            env.outerEnv = temp;
        }

        /// <summary>
        /// 向当前环境和其外部环境中间插入一个环境链
        /// (注意,插入的环境可能会换掉其直接外部环境)
        /// </summary>
        /// <param name="env">中介环境链</param>
        public void InsertEnvLink(Environment env)
        {
            //这里可能有隐患,请注意！
            //隐患就是,env的直接外部环境可能被换掉
            if (env == null) return;
            Environment temp = outerEnv;
            NestedEnv rear = env as NestedEnv;
            //2->3->4 |
            //        |=> 1->2->3->4
            //1->4    |
            //--------------------------
            //3->5->0(super对象)     |
            //(上插到2后面)          |=>2->3->5->0->4->5->0->3->...(期望:2->3->4->5->0)
            //2->4->5->0(this对象)   |
            while (rear.outerEnv != null && rear.outerEnv != temp)
                rear = rear.outerEnv as NestedEnv;
            rear.outerEnv = temp;
            outerEnv = env;
        }

        public void CopyFrom(IDictionary<string, object> table)
        {
            foreach(var name in table.Keys)
            {
                if (!variables.ContainsKey(name))
                    variables.Add(name, table[name]);
            }
        }

        public void CopyFrom(object obj)
        {
            if(obj is NestedEnv)
                CopyFrom((obj as NestedEnv).variables);
        }

        /// <summary>
        /// 改变该环境中的变量名称
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void UpdateName(string oldName, string newName)
        {
            if (Contains(oldName))
            {
                object val = variables[oldName];
                variables.Remove(oldName);
                PutInside(newName, val);
            }
        }
    }

    public class ArrayEnv : Environment
    {
        protected object[] values;
        protected Environment outer;

        public string[] Names
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object this[string varName]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ArrayEnv(int size, Environment outer)
        {
            values = new object[size];
            this.outer = outer;
        }

        //nest为相对嵌套层次数,本层为0,外层为0+n
        public object Get(int nest, int index)
        {
            if (nest == 0) return values[index];
            else if (outer == null) return null;
            else return (outer as ArrayEnv).Get(nest - 1, index);
        }

        public void Set(int nest, int index, object value)
        {
            if (nest == 0 || outer == null) values[index] = value;
            else (outer as ArrayEnv).Set(nest - 1, index, value);
        }

        public bool Contains(string varName)
        {
            throw new NotImplementedException();
        }

        public object Get(string varName)
        {
            throw new NotImplementedException();
        }

        public void Put(string varName, object value)
        {
            throw new NotImplementedException();
        }

        public void PutInside(string name, object value)
        {
            throw new NotImplementedException();
        }

        public Environment Where(string name)
        {
            throw new NotImplementedException();
        }

        public bool RemoveInside(string name)
        {
            throw new NotImplementedException();
        }

        public string[] GetNames()
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IDictionary<string, object> table)
        {
            throw new NotImplementedException();
        }

        public void UpdateName(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string name)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(object obj)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string varName)
        {
            throw new NotImplementedException();
        }

        public object GetInside(string varName)
        {
            throw new NotImplementedException();
        }

        public T GetInside<T>(string varName)
        {
            throw new NotImplementedException();
        }
    }

    public class DynaArrayEnv : ArrayEnv
    {
        public DynaArrayEnv(int size, Environment outer) 
            : base(size, outer){ }

    }

    public class Sysmbols
    {
        public class Location
        {
            public int Nest { get; set; }
            public int Index { get; set; }
            public Location(int nest, int index)
            {
                Nest = nest; Index = index;
            }
        }

        protected Sysmbols outer;
        protected Dictionary<string, int> table 
              = new Dictionary<string, int>(32);
        public int Size { get { return table.Count; } }

        public Sysmbols() : this(null) { }
        public Sysmbols(Sysmbols outer)
        {
            this.outer = outer;
        }

        public void Append(Sysmbols outer)
        {
            foreach(var pair in outer.table)
            {
                if (!table.ContainsKey(pair.Key))
                    table.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Get a variable's index within this env, and return index.if that doesn't exist, return -1;
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public int Find(string varName)
        {
            int index = -1;
            if (table.TryGetValue(varName, out index))
                return index;
            return -1;
        }

        public Location Get(string varName)
        {
            return Get(varName, 0);
        }

        public Location Get(string varName, int nest)
        {
            int index = -1;
            if (table.TryGetValue(varName, out index))
                return new Location(nest, index);
            else
            {
                if (outer == null) return null;
                else return outer.Get(varName);
            }
        }

        public int PutNew(string varName)
        {
            int i = Find(varName);
            if (i == -1)
                return Add(varName);
            else return i;
        }

        public Location Put(string varName)
        {
            Location loc = Get(varName);
            if (loc == null)
                return new Location(0, Add(varName));
            else return loc;
        }

        protected int Add(string varName)
        {
            int i = -1;
            if (!table.ContainsKey(varName))
            {
                i = Size;
                table.Add(varName, i);

            }
            else i = table[varName];
            return i;
        }
    }
}
