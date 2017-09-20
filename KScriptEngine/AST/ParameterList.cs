using KScript.KSystem.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ParameterList : ASTList
    {
        public bool IsVarParams { get; set; }

        public IEnumerable<string> ParamNames
        {
            get
            {
                for (int i = 0; i < Length; i++)
                    yield return ParamName(i);
            }
        }

        public ParameterList(List<ASTree> list) : base(list)
        {
            IsVarParams = list.Where(param => param.ChildrenCount > 1)
                              .Count() > 0;
        }

        public string ParamName(int i)
        {
            //return (children[i] as ASTLeaf).Token.Text;
            return ParamName(Param(i));
        }

        public static string ParamName(ASTree origin)
        {
            return ((origin.ChildrenCount == 1 ? origin : origin[1])
                     as ASTLeaf).Text;
        }

        public ASTree Param(int i)
        {
            if (i >= Length - 1)
                i = Length - 1;
            return children[i];
        }

        public int Length
        { get { return ChildrenCount; } }

        /// <summary>
        /// 为形参数表传递值,放置在执行函数的作用域内
        /// </summary>
        /// <param name="env">调用函数的作用域</param>
        /// <param name="index">参数索引</param>
        /// <param name="value">为对应参数传递的值</param>
        public void Evaluate(Environment env, int index, object value)
        {
            var param = Param(index);
            string name = ParamName(index);
            //判断组成此参数的第一个单词是不是修饰符
            if (param[0] is Modifier)
            {
                var list = env.Get<KList>(name);
                int liIndex = index - Length + 1;
                if (index >= list.Count)
                    list.Add(value);
                else list[liIndex] = value;
                value = list;
            }
            env.PutInside(name, value);
        }

        public void IniVarParams(Environment env)
        {
            //创建变长参数列表
            if (IsVarParams)
            {
                //此处Length只是取一个超过边界值的值,确保取到最后一个参数
                env.PutInside(ParamName(Length), new KList(8));
            }
        }

        public void AssertIsLenMatch(int realLen, int errorLoc = 0)
        {
            int minLen = IsVarParams ? Length - 1 : Length;
            if (realLen < minLen)
                throw new KException("Wrong arguments' length.", errorLoc);
        }

        public override object Clone()
        {
            return new ParameterList(children);
        }
    }
}
