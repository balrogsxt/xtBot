using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class Favicon : GroupEventApi, GroupEvents {
        public void run() {
            if (this.checkSleep(5)) return;


            string apiUrl = "https://api.acgxt.com/favicons";

            string value = this.getValue();


            if (!Util.isUrl(value)) {
                this.sendMessage(CQ.at(this.fromQQ) + "请输入正确的网址!");
                return;
            }

            string url = String.Format("{0}?url={1}", apiUrl, value);
            string data = Util.httpGet(url, 5000);
            if (data=="NULL") {
                this.sendMessage(CQ.at(this.fromQQ)+"获取Favicon失败");
                return;
            }
            if (data== "https://api.acgxt.com/Public/favicon.null.ico") {
                this.sendMessage(CQ.at(this.fromQQ) + "没有获取到该网站的Favicon图标");
                return;
            }
            
            this.sendMessage(CQ.httpImage(data, "png"));



        }
    }
}
