using KScript.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KScript.AST
{
    public abstract class ASTree : IEnumerable<ASTree>, ICloneable
    {
        public static readonly int UNKNOW_LINE = 0;
        //定义最大循环次数
        public static readonly int STACK_MAXLEVEL = int.MaxValue;
        public static readonly Dictionary<bool, double> BOOL = new Dictionary<bool, double>()
        {
            { true, 1D }, {  false, 0D }
        };

        public bool IsEmpty
        {
            get { return this is ASTList && ChildrenCount == 0; }
        }

        public abstract int LowerBound { get; }
        public abstract int UpperBound { get; }
        public abstract int LineNo { get; }

        public abstract int ChildrenCount { get; }
        public abstract IEnumerable<ASTree> Children { get; }
        public abstract ASTree this[int i] { get; }
        public abstract IEnumerator<ASTree> GetEnumerator();

        /// <summary>
        /// 执行AST,并返回值
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public virtual object Evaluate(Environment env)
        {
            //throw new NotImplementedException("Can not eval AST!");
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString() { return ""; }

        public virtual object Clone()
        {
            throw new NotImplementedException("Can't be cloned!");
        }

        /// <summary>
        /// Visitor模式的接口,执行一些拓展操作(可以用拓展方法代替,此处只是尝试一下设计模式)
        /// </summary>
        /// <param name="visit">定义了具体拓展方案的对象</param>
        public virtual void Accept(Visitor visit)
        {
            visit.Visit(this);
        }
    }
}
