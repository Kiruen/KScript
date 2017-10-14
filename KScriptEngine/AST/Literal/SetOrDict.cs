using KScript.Runtime;
using KScript.KSystem.BuiltIn;
using System.Collections.Generic;
using System.Linq;

namespace KScript.AST
{
    public class DictLiteral : ASTList
    {
        public DictLiteral(List<ASTree> list) : base(list)
        { }

        public override object Evaluate(Environment env)
        {
            //为什么先addtoken然后ast,当后面的值表为单组表达式时,不会
            //保留单组表达式的ASTList,而不像那样做,就会保留??
            //还在纳闷为什么不能用拓展方法呢。。原来是没有使用Linq命名空间
            if (children.Count > 0 && children[0] is BinaryExpr &&
                (children[0] as BinaryExpr).Operator == ":")
                return new KDict(children
                    .Select(expr => expr.Evaluate(env) as KTuple)
                    .ToDictionary(tuple => tuple[0], tuple => tuple[1]));
            else
                return new KSet(children.Select
                        (expr => expr.Evaluate(env)));

            //KDict dict = new KDict();
            ////获取字典构造体
            //var body = children[0];
            ////这里是判断是否为单个键值对(由于遗留问题,当只有一对时不会将键值对当成整体看待)
            //if (body.ChildrenCount != 0 && !(body[0] is Pair))
            //    body = new ASTList(children);
            //foreach (var pair in body)
            //{
            //    var _pair = pair.Evaluate(env) as Tuple<object, object>;
            //    dict[_pair.Item1] = _pair.Item2;
            //}
            //return dict;
        }
    }

    public class SetLiteral : ASTList
    {
        public SetLiteral(List<ASTree> list) : base(list)
        { }

        public override object Evaluate(Environment env)
        {
            return new KSet(children
                .Select(expr => expr.Evaluate(env)));
        }
    }
}
