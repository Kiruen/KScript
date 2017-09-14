using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript.AST
{
    public class ExceptionStmnt : ASTList
    {
        public BlockStmnt Try
        {
            get { return this[0] as BlockStmnt; }
        }
        public ASTree ExcVar
        {
            get { return this[1]; }
        }
        public BlockStmnt Catch
        {
            get { return this[2] as BlockStmnt; }
        }

        public ExceptionStmnt(List<ASTree> list) : base(list)
        { }

        public override object Evaluate(Environment env)
        {
            try
            {
                Try.Evaluate(env);
            }
            catch(Exception exc)
            {
                Environment inner = new NestedEnv(env);
                //包装异常对象
                var excObj = NativeObject.Pack(exc, inner);
                //if(exc is KException)
                //    excObj.Write("location", (exc as KException).)
                if (ExcVar is ASTLeaf) //!(ExceptionVar is NullStmnt)
                {
                    inner.PutInside((ExcVar as ASTLeaf).Text , excObj);
                }
                Catch.Evaluate(inner);
            }
            return null;
        }
    }
}
