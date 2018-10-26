using com.acgxt.bot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Core {
    class App {

        private static int authCode = 0;
        /// <summary>
        /// 获取Authcode
        /// </summary>
        /// <returns></returns>
        public static int getAuthCode() {
            if (App.authCode==0) {
                App.authCode = CQAPI.getCQPAuthCode();
                if (App.authCode!=0) {
                    Log.addLog("SET_AUTHCODE","已获取最新AuthCode:"+App.authCode);
                }
            }
            return App.authCode;
        }
 



    }
}
