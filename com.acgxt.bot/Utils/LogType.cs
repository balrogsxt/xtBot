using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    public enum LoggerType {
        DEBUG = 0,
        INFO = 10,
        SUCCESS = 11,
        WARNING = 20,
        ERROR = 30,
        FATAL = 40
    }
    class LogType {

        public enum status : int {
            DEBUG=0,
            INFO=10,
            SUCCESS=11,
            WARNING=20,
            ERROR=30,
            FATAL=40
        };

    }
}
