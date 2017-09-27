using KScript.Callable;
using KScript.KSystem;
using KScript.KSystem.BuiltIn;
using KScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KScript.AST
{
    public class BinaryExpr : ASTList
    {
        public BinaryExpr(List<ASTree> list) : base(list)
        { }

        public ASTree Left
        {
            get { return this[0]; }
        }
        public string Operator
        {
            get { return ((ASTLeaf)this[1]).Text; }
        }
        public ASTree Right
        {
            get { return this[2]; }
        }

        public override object Evaluate(Environment env)
        {
            if("=" == Operator)
                return Assign(env, Right.Evaluate(env));
            else if (BasicParser.operators[Operator].Level == BasicParser.ASSIGN_OP 
                                              && Operator[Operator.Length - 1] == '=')
                //复合赋值运算符
                return ComputeThenAssign(env, Operator, Right.Evaluate(env));
            else if (Operator == "&&" || Operator == "||" || 
                        Operator == "and" || Operator == "or")
            {
                Func<object> callBack = () => Right.Evaluate(env);
                return ComputeLazy(Left.Evaluate(env), Operator, callBack);
            }
            else
                return Compute(Left.Evaluate(env), Operator, Right.Evaluate(env));
        }

        protected object Assign(Environment env, object rValue)
        {
            if (Left is VarName)
            {
                env.Put((Left as VarName).Name, rValue);
            }
            else if(Left is PrimaryExpr)    //It means that left is an member_access(句点引用)
            {
                PrimaryExpr prefix = Left as PrimaryExpr;
                //如果有后缀
                if(prefix.HasPostfix(0))
                {
                    Postfix postfix = prefix.Postfix(0);
                    /*obj.func1().x = 3
                      => <obj.func1()>.x = 3
                      => <<obj.func1>()>.x = 3
                      => <obj_func1()>.x = 3
                      => p.x = 3 */
                    if (postfix is Dot)
                    {
                        object access = prefix.EvalSubExpr(env, 1);
                        if (access is KObject)
                            return SetField((KObject)access, (Dot)prefix.Postfix(0), rValue);
                    }
                    else if(postfix is ArrayRef)
                    {
                        object obj = prefix.EvalSubExpr(env, 1);
                        ArrayRef arrayRef = postfix as ArrayRef;
                        object index = arrayRef.Index.Evaluate(env);
                        if (obj is object[])
                        {
                            (obj as object[])[Convert.ToInt32(index)] = rValue;
                        }
                        else if (obj is List<object>)
                        {
                            (obj as List<object>)[Convert.ToInt32(index)] = rValue;
                        }
                        //else if (obj is Indexable)
                        //{
                        //    (obj as Indexable)[index] = rValue;
                        //}
                        //调用索引器
                        else if (obj is KObject)
                        {
                            var method = obj.GetType().GetMethod("set_Item");
                            if (method != null)
                                method.Invoke(obj, new[] { index, rValue });
                            else
                                Arguments.Call((obj as KObject)
                                    .Read<IFunction>("setter"), env, index, rValue);
                        }
                    }
                }
            }
            else
                throw new KException("Bad assignment", LineNo);
            return rValue;
        }

        protected object Compute(object left, string op, object right)
        {
            //为什么会有无效的转换？反射得到的值到底是什么类型的？↓
            //其实是因为未进行拆箱操作,需要:obj->double->int
            if (/*op != ":" && */left != null && right != null &&
                left.GetType().IsValueType && right.GetType().IsValueType)
                return ComputeNum(Convert.ToDouble(left), op, Convert.ToDouble(right));
            //else if (op == "+" && (left is KString || right is KString))
            //    return KString.Instance
            //            (KUtil.ToString(left) + KUtil.ToString(right));
            else if (op == "==" || op == "!=")
            {
                bool eq = op.Equals("==");
                if (left == null)
                    return eq ? BOOL[right == null] : BOOL[right != null];
                else
                    return eq ? BOOL[left.Equals(right)] : BOOL[!left.Equals(right)]; //left == tight is wrong : use "object" operator reloading
            }
            //已使用运算符重载实现
            //else if (op == "*" && left is KString && right.GetType().IsValueType)
            //{
            //    var result = new StringBuilder();
            //    double len = Convert.ToDouble(right), count = 0;
            //    while (count++ < len)
            //        result.Append(left);
            //    return KString.Instance(result);
            //}
            //TODO:实现运算符重载
            else
                return ComputeObj(left, op, right);
            //throw new KException("Cannot compute: Bad type", Location);
        }

        protected object ComputeNum(double left, string op, double right)
        {
            switch (op)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": return left / right; // /0 will throw an exception
                case "%": return left % right;
                case ":": return new KTuple(left, right);
                case "&": return (int)left & (int)right;
                case "|": return (int)left | (int)right;
                case "^": return (int)left ^ (int)right;
                case ">>": return (int)left >> (int)right;
                case "<<": return (int)left << (int)right;
                case "**": return Math.Pow(left, right);
                case "==": return BOOL[left == right];
                case "!=": return BOOL[left != right];
                case ">": return BOOL[left > right];
                case ">=": return BOOL[left >= right];
                case "<": return BOOL[left < right];
                case "<=": return BOOL[left <= right];
                case "~": return new KRange("[", left, right, "]");
                default: throw new KException("Bad compution", LineNo);
            }
        }

        protected object ComputeLazy(object left, string op, Func<object> rightCall)
        {
            switch (op)
            {
                case "||": case "or": return ToDouble(left) != 0 ? BOOL[true] : BOOL[ToDouble(rightCall()) != 0];
                case "&&": case "and": return ToDouble(left) == 0 ? BOOL[false] : BOOL[ToDouble(rightCall()) != 0];
                //case ":": return new KTuple(left, rightCall);
                default: throw new KException("Bad compution", LineNo);
            }
        }

        protected object ComputeObj(object left, string op, object right)
        {
            switch (op)
            {
                case ":": return new KTuple(left, right);
                case "?":
                    {
                        var tuple = right as KTuple;
                        return (double)left != 0 ? tuple[0] : tuple[1];
                    }
                case "in":
                    {
                        bool res;
                        if (right is KString || right is string)
                            res = right.ToString().Contains(left.ToString());
                        else if (right is KRange)
                            res = (right as KRange).Contains((double)left);
                        else
                            res = (right as IEnumerable<object>).Contains(left);
                        return BOOL[res];
                    }
                case "is":
                    {
                        if(right is ClassInfo)
                        {
                            //var kobj = left as KObject;
                            bool res = API.TypeOf(left) == right;
                            if(left is KObject)
                            {
                                var kobj = left as KObject;
                                while (!res && (kobj = kobj.TryRead<KObject>("super")) != null)
                                {
                                    res |= API.TypeOf(kobj) == right;
                                }
                            }
                            return BOOL[res];
                        }
                        return 0D;
                    }
                //TODO:支持范围对象的集合运算
                case "∩": return (left as KSet).Intersect(right as KSet);
                case "∪": return (left as KSet).Union(right as KSet);
                case "⊂": return (left as KSet).PropInclude(right as KSet);
                case "⊆": return (left as KSet).Include(right as KSet);
                case "∈": return (right as KSet).Contians(left);
                //运算符重载
                case "+": return InvokeOpOverload(left, "_add", right);
                case "-": return InvokeOpOverload(left, "_sub", right);
                case "*": return InvokeOpOverload(left, "_mul", right);
                case "/": return InvokeOpOverload(left, "_div", right);
                case "**": return InvokeOpOverload(left, "_dmul", right);
                default: throw new KException("Bad compution", LineNo);
            }
        }

        protected object ComputeThenAssign(Environment ev, string op, object right)
        {
            op = op.Substring(0, op.Length - 1);
            object result = Compute(Left.Evaluate(ev), op, right);
            return Assign(ev, result);
        }

        //为对象的成员字段赋值
        protected object SetField(KObject obj, Dot expr, object rvalue)
        {
            string name = expr.Name;    //获取.后面的成员名称
            try
            {
                obj.Write(name, rvalue);
                return rvalue;
            }
            catch {
                throw new KException("Bad member access: " + name, LineNo);
            }
        }

        protected object InvokeOpOverload(object left, string olName, object right)
        {
            KObject invoker = null;
            IFunction func = null;
            object arg = right;
            if(left is KObject)
            {
                invoker = left as KObject;
                func = invoker.TryRead<IFunction>(olName);
            }
            if(right is KObject && func == null)
            {
                invoker = right as KObject;
                func = invoker.TryRead<IFunction>(olName);
                arg = left;
            }
            //if(!(left is KObject) && right is KObject)
            //{
            //    var temp = right;
            //    right = left;
            //    left = temp;
            //}
            //var obj = left as KObject;
            //var func = obj.TryRead(olName);
            if(func != null)
            {
                //由于此为常量之间的运算(即使是变量也会先去取得),因此不需要指定调用环境
                return func.Invoke(null, arg);
                //return Arguments.Call(func, null, param);
            }
            throw new KException("Unsupported operator overload!", LineNo);
        }

        private static double ToDouble(object val)
        {
            return Convert.ToDouble(val);
        }

        //表示分支结构的选择树(用于三目运算符?:)
        //已弃用,用KTuple代替
        //public class Option
        //{
        //    public object Left, Right;
        //    public Option(object left, object right)
        //    {
        //        Left = left; Right = right;
        //    }
        //}
    }
}
