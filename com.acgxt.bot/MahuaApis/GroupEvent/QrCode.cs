using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class QrCode : GroupEventApi, GroupEvents {



        public void run() {
            if (this.checkSleep(4)) return;

            string apiUrl = "https://api.acgxt.com/qr";

            string value = this.getValue();


            if (Util.checkEmpty(this.value)) {
                this.sendMessage(CQ.at(this.fromQQ) + "请输入二维码的信息!");
                return;
            }
            string url = String.Format("{0}?key={1}&size=20&margin=0", apiUrl, value);
            this.sendMessage(CQ.httpImage(url,"png"));
            
        }
    }
}
