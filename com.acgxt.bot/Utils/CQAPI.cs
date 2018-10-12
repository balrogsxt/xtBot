using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    class CQAPI {
        //获取authCode
        [DllImport("com.acgxt.cqp.e.dll")]
        public static extern int getAuthCode();
        //撤回消息API
        [DllImport("com.acgxt.cqp.e.dll")]
        public static extern int xtDeleteMessage(int ac, long msgid);
        //添加日志
        [DllImport("com.acgxt.cqp.e.dll", EntryPoint = "xtAddLog")]
        public static extern int xtAddCQPLog(int ac, LogType.status state, string type, string content);

        [DllImport("com.acgxt.cqp.e.dll")]
        public static extern string getGroupUserList(int ac, long group);

        

        public static int xtAddLog(int ac,LogType.status state,string type,string content) {
            Log.addLog("CQAPI",type+":"+content);
            return xtAddCQPLog(ac,state,type,content);
        }


    }
}
