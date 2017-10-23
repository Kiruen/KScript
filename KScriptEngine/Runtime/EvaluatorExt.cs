using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KScript
{
    public partial class Evaluator
    {
        private StringBuilder scriptTemp;

        public Evaluator()
        {
            scriptTemp = new StringBuilder();
        }

        /// <summary>
        /// 向执行器尾插入一段完整的脚本代码,将推迟执行。
        /// </summary>
        /// <param name="script"></param>
        public void InsertCode(string script)
        {
            scriptTemp.AppendLine(script);
        }

        /// <summary>
        /// 插入最后一段代码,锁定脚本内容,立即执行词法分析,并准备执行脚本内容。
        /// </summary>
        /// <param name="script">插入的脚本内容</param>
        /// <param name="clean">指定是否清空脚本缓存.在进行单元测试和交互式执行时,建议使用默认值来清空脚本缓存</param>
        public void InsertLastCode(string script, bool clean = true)
        {
            InsertCode(script);
            lexer = new Lexer(scriptTemp.ToString());
            lexer.ReadAll();
            if (clean)
            {
                scriptTemp.Clear();
            }
        }
    }
}
