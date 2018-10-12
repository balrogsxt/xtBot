using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils.XtException {
    public class ReReplyException : ApplicationException {
        private string msg;
        private Exception innerException;
        public ReReplyException() {
            
        }
        public ReReplyException(string msg) : base(msg) {
            this.msg = msg;
        }
        //带有一个字符串参数和一个内部异常信息参数的构造函数
        public ReReplyException(string msg, Exception innerException):base(msg){
            this.innerException=innerException;
            this.msg = msg;
        }
        public string getMsg() {
            return this.msg;
        }

    }
}
