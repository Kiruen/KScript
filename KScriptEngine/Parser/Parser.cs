using KScript.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace KScript
{
    public class Parser
    {
        public string Name { get; protected set; }
        protected List<Element> elements;
        protected Factory factory;

        public Parser(Type clazz)
        {

            Reset(clazz);
        }

        public Parser(Parser p)
        {
            elements = p.elements;
            factory = p.factory;
        }

        public static Parser Rule()
        {
            return Rule(null);
        }

        public static Parser Rule(Type nodeType)
        {
            return new Parser(nodeType) { Name = nodeType?.Name ?? "Unknown" };
        }

        public static Parser Rule<T>()
        {
            return new Parser(typeof(T));
        }

        public bool Match(Lexer lexer)
        {
            //非常关键！！！若试图匹配个空的Parser,则直接返回true
            //下面的parse会返回一个对应子树类型的空语法树(比如代表空的参数表)
            if (elements.Count == 0)
                return true;
            else
            {
                Element e = elements[0];
                return e.Match(lexer);
                //Element e;
                //if (match_time == 2)
                //{ ; }
                //for (int i = 0; i < match_time; i++)
                //{
                //    e = elements[i];
                //    if (!e.Match(lexer))
                //        return false;
                //}
                //return true;

                //var e = elements[0];
                //if (e is Skip)
                //{
                //    for (int i = 1; i < 3 && e is Skip; i++)
                //    {
                //        if (!e.Match(lexer))
                //            return false;
                //        e = elements[i];
                //    }
                //    return true;
                //}
                //else
                //    return e.Match(lexer);
            }
        }

        public ASTree Parse(Lexer lexer)
        {
            List<ASTree> results = new List<ASTree>();
            foreach (Element e in elements)
                e.Parse(lexer, results);

            return factory.CreateRoot(results);
        }

        public Parser Reset()
        {
            elements = new List<Element>();
            return this;
        }
        public Parser Reset(Type clazz)
        {
            elements = new List<Element>();
            factory = Factory.GetFactoryForASTList(clazz);
            return this;
        }

        public Parser Or(params Parser[] parsers)
        {
            elements.Add(new OrTree(parsers));
            return this;
        }

        public Parser Ast(Parser parser)
        {
            elements.Add(new Tree(parser));
            return this;
        }

        public Parser AddToken(params string[] pat)
        {
            elements.Add(new Finality(pat));
            return this;
        }

        public Parser AddNum(Type nodeType)
        {
            elements.Add(new ANumber(nodeType));
            return this;
        }

        /// <summary>
        /// 添加一个标识符(纯粹的单词,不代表已存在的变量)
        /// </summary>
        /// <param name="reserved"></param>
        /// <returns></returns>
        public Parser AddId(HashSet<string> reserved)
        {
            elements.Add(new AnId(null, reserved));
            return this;
        }

        /// <summary>
        /// 添加一个标识符(代表某个变量)
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="reserved"></param>
        /// <returns></returns>
        public Parser AddId(Type nodeType, HashSet<string> reserved)
        {
            elements.Add(new AnId(nodeType, reserved));
            return this;
        }

        public Parser AddStr(Type nodeType)
        {
            elements.Add(new AString(nodeType));
            return this;
        }

        public Parser Expression(Type nodeType, Parser subExp, Operators ops)
        {
            elements.Add(new Expr(nodeType, subExp, ops));
            return this;
        }

        public Parser Sep(params string[] pat)
        {
            elements.Add(new Skip(pat));
            return this;
        }

        //Option与Maybe的区别:若碰到空的参数表,则前者不会生成代表参数表的子树
        //(实际上参数表为空不应该省略,被省略的话就意味着我们无法得具体的语法类型)
        //而后者至少会生成一棵仅有根结点的空子树(详见repeat和ortree的parse方法)
        public Parser Option(Parser p)
        {
            elements.Add(new Repeat(p, true));
            return this;
        }

        /// <summary>
        /// Option的升级版,当未匹配时,提供一个占位符
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Parser Maybe(Parser p)
        {
            Parser p2 = new Parser(p);
            p2.Reset();
            elements.Add(new OrTree(new Parser[] { p, p2 }));
            return this;
        }

        public Parser Rep(Parser p)
        {
            elements.Add(new Repeat(p, false));
            return this;
        }

        public Parser InsertChoice(Parser p)
        {
            Element e = elements[0];
            if (e is OrTree)
                ((OrTree)e).Insert(p);
            else
            {
                Parser otherwise = new Parser(this); //拷贝elements
                Reset(null);        //置空本体
                Or(p, otherwise);   //把插入语法和原语法组成OrTree然后重新放人elements中
            }
            return this;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public abstract class Element
        {
            public abstract void Parse(Lexer lexer, List<ASTree> res);
            public abstract bool Match(Lexer lexer);
            public override string ToString()
            {
                return $"{GetType().Name}";
            }
        }

        public class Repeat : Element
        {
            protected Parser parser;
            protected bool onlyOnce;
            public Repeat(Parser p, bool once) { parser = p; onlyOnce = once; }
            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                while (parser.Match(lexer))
                {
                    //之前分析函数调用语句时没有
                    ASTree ast = parser.Parse(lexer);
                    //表示三种类型:
                    //①ASTLeaf
                    //②原版Java代码的glounj拓展类(包含一些仅含有根结点的ASTList)
                    //这样的移植可能存在隐患,拓展时应予以重点维护
                    if (!(ast is ASTList) || ast.ChildrenCount > 0 
                       || ast is Arguments)
                        res.Add(ast);
                    if (onlyOnce) break;
                }
            }

            public override bool Match(Lexer lexer)
            {
                return parser.Match(lexer);
            }
        }

        /// <summary>
        /// 代表不可再分割的终结符
        /// </summary>
        private class Finality : Element
        {
            private HashSet<string> patterns;
            public Finality(string[] tokens)
            {
                patterns = new HashSet<string>(tokens);
            }

            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                Token token = lexer.Read();
                if (token is IdToken || token is SymToken) //  || token.Text == Token.EOL
                {
                    if (patterns.Contains(token.Text))
                    {
                        OnTokenFound(res, token);
                        return;
                    }
                }
                //没有匹配到任何模式终结符
                if (patterns.Count > 0)
                    throw new ParseException("Patterns all failed: " + patterns.ElementAt(0) + " is expected.", token);
                else
                    throw new ParseException(token);
            }

            public override bool Match(Lexer lexer)
            {
                Token token = lexer.Peek(0);
                if (token is IdToken || token is SymToken) //  || token.Text == Token.EOL
                {
                    return patterns.Contains(token.Text);
                    //foreach (var pattern in patterns)
                    //    if (pattern == token.Text)
                    //        return true;
                }
                return false;
            }

            protected virtual void OnTokenFound(List<ASTree> res, Token t)
            {
                res.Add(new ASTLeaf(t));
            }
        }

        private class Skip : Finality
        {
            public Skip(string[] pat) : base(pat)
            { }

            protected override void OnTokenFound(List<ASTree> res, Token t)
            { /* Skip: Do nothing. */ }
        }

        private class OrTree : Element
        {
            protected Parser[] parsers;
            public OrTree(Parser[] parsers)
            {
                this.parsers = parsers;
            }
            public override bool Match(Lexer lexer)
            {
                return Choose(lexer) != null;
            }

            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                Parser parser = Choose(lexer);
                if(parser == null)
                    throw new ParseException(lexer.Peek(0));
                res.Add(parser.Parse(lexer));
            }

            public Parser Choose(Lexer lexer)
            {
                foreach (var parser in parsers)
                    if (parser.Match(lexer))
                        return parser;
                return null;
            }

            //头插入一条语法
            public void Insert(Parser parser)
            {
                Parser[] newParsers = new Parser[parsers.Length + 1];
                newParsers[0] = parser;
                Array.Copy(parsers, 0, newParsers, 1, parsers.Length);
                parsers = newParsers;
            }
        }

        private class Tree : Element
        {
            protected Parser parser;
            public Tree(Parser parser)
            {
                this.parser = parser;
            }

            public override bool Match(Lexer lexer)
            {
                return parser.Match(lexer);
            }

            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                res.Add(parser.Parse(lexer));
            }
        }

        public abstract class AToken : Element
        {
            protected Factory factory;
            protected AToken(Type type)
            {
                if (type == null)
                    type = typeof(ASTLeaf);
                factory = Factory.GetFactory(type, typeof(Token));
            }

            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                Token t = lexer.Read();
                if (Check(t)) //这里还需要check吗？不是先match再parse吗
                {
                    ASTree leaf = factory.CreateRoot(t);
                    res.Add(leaf);
                }
                else
                    throw new ParseException(t);
            }

            public override bool Match(Lexer lexer)
            {
                return Check(lexer.Peek(0));
            }

            protected abstract bool Check(Token t); //检查单词是否满足要求
        }

        public class AnId : AToken
        {
            HashSet<string> reserved; //保留字
            public AnId(Type type, HashSet<string> r) : base(type)
            {
                reserved = r != null ? r : new HashSet<string>();
            }

            protected override bool Check(Token t)
            {
                //检查是否是标识符,且不应该是保留字
                //t.isIdentifier()
                return t is IdToken && !reserved.Contains(t.Text);
            }
        }

        public class ANumber : AToken
        {
            public ANumber(Type type) : base(type) { }
            protected override bool Check(Token t) { return t.IsNumber(); }
        }

        public class AString : AToken
        {
            public AString(Type type) : base(type) { }
            protected override bool Check(Token t) { return t.IsString(); }
        }

        //优先级
        public class Precedence
        {
            public int Level { get; set; }
            public bool LeftAssoc { get; set; } // left associative 左结合性
            public Precedence(int level, bool lftAss)
            {
                Level = level; LeftAssoc = lftAss;
            }
        }

        public class Operators : Dictionary<string, Precedence>
        {
            public static bool LEFT = true;
            public static bool RIGHT = false;
            public void Add(string name, int prec, bool leftAssoc)
            {
                base.Add(name, new Precedence(prec, leftAssoc));
            }
        }

        public class Expr : Element
        {
            protected Factory factory;
            protected Operators ops;
            protected Parser factor;

            //subExp:表示Bin表达式的项的Parser
            public Expr(Type nodeType, Parser subExp, Operators opMap)
            {
                factory = Factory.GetFactoryForASTList(nodeType);
                ops = opMap;
                factor = subExp;
            }

            public override void Parse(Lexer lexer, List<ASTree> res)
            {
                //这里指右子树,而不是右面的表达式
                ASTree right = factor.Parse(lexer); //分析一个因子
                Precedence prec;
                while ((prec = NextOperator(lexer)) != null) //检测到运算符
                    right = DoShift(lexer, right, prec.Level); 
                //注意！虽然叫binary,但第二个值也可以为空的
                res.Add(right);
            }

            private ASTree DoShift(Lexer lexer, ASTree left, int prec)
            {
                List<ASTree> list = new List<ASTree>();
                list.Add(left);
                list.Add(new ASTLeaf(lexer.Read())); //读取运算符
                ASTree right = factor.Parse(lexer);
                Precedence next;
                //如果后面的运算符优先级比它小,则至此截断,用此Bin表达式构造一个AST
                //返回给上层,然后此AST将作为上层(Parse())的"左"子树继续做Shift(),直到整棵树构造完成
                //如果后面的运算符优先级大于等于它,则从right前面截断,然后从right后面开始
                //继续构造.DoShift的意思应该是:如果此表达式比后面优先,则构成AST向左上方传
                //否则,就向右下方深入
                while ((next = NextOperator(lexer)) != null
                                && RightIsExpr(prec, next))
                    right = DoShift(lexer, right, next.Level); //递归

                list.Add(right);
                return factory.CreateRoot(list); //返回Bin表达式的根结点(实际上是持有这个表达式的AST)
            }

            private Precedence NextOperator(Lexer lexer)
            {
                Token t = lexer.Peek(0);
                //t.isIdentifier
                if (t is SymToken && ops.ContainsKey(t.Text))
                    return ops[t.Text];
                else
                    return null;
            }

            private static bool RightIsExpr(int prec, Precedence nextPrec)
            {
                if (nextPrec.LeftAssoc)
                    return prec < nextPrec.Level; //若优先级相同,则先出现的先结合
                else
                    return prec <= nextPrec.Level;  //若优先级相同,则最后出现的表达式先结合
            }

            public override bool Match(Lexer lexer)
            {
                return factor.Match(lexer);
            }
        }

        /*public class Mono : Element
        {
            protected Factory factory;
            protected Operators ops;
            protected Parser factor;
        }*/
    }

    public class Factory
    {
        public const string factoryName = "Create"; //简化构造的方法
        protected Func<object, ASTree> createRoot;
        public ASTree CreateRoot(object arg)
        {
            return createRoot(arg);
        }

        public static Factory GetFactoryForASTList(Type clazz)
        {
            Factory factory = GetFactory(clazz, typeof(List<ASTree>));
            if (factory == null)
                factory = new Factory()
                {
                    createRoot = arg =>
                    {
                        //如果此树只有一个孩子,则直接返回孩子
                        List<ASTree> results = (List<ASTree>)arg;
                        if (results.Count == 1)
                            return results[0];
                        else
                            return new ASTList(results);
                    }
                };
            return factory;
        }

        public static Factory GetFactory(Type clazz, Type argType)
        {
            if (clazz == null)
                return null;
            try
            {
                MethodInfo m = clazz.GetMethod(factoryName, new Type[] { argType });
                if (m == null) throw new Exception("未找到方法");
                return new Factory()
                {
                    createRoot = arg => (ASTree)m.Invoke(null, new object[] { arg })
                };
            }
            catch { }
            try
            {
                ConstructorInfo c = clazz.GetConstructor(new Type[] { argType });
                return new Factory()
                {
                    createRoot = arg => { return (ASTree)c.Invoke(new object[] { arg }); }
                };
            }
            catch { }
            return null;
        }
    }
}
