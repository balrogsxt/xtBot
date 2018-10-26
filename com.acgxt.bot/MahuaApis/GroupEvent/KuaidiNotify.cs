using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    /// <summary>
    /// 快递通知
    /// </summary>
    class KuaidiNotify : GroupEventApi, GroupEvents {

        public void run() {
            if (this.checkSleep(2)) return;


            string number = this.getValue();
            string kdType = String.Empty;
            string data = String.Empty;
            JObject json;




            //验证是否存在缓存
            object cache = Conf.getConfig("global.kuaidi." + this.fromGroup, this.fromQQ.ToString());
            bool flag = false;
            if (cache != null) {
                if (Util.isJson(cache.ToString())) {
                    json = JObject.Parse(cache.ToString());

                    if (json.Property("number") != null && json.Property("type") != null) {
                        number = json["number"].ToString();
                        kdType = json["type"].ToString();
                        flag = true;
                    }
                }
            }

            if (!flag) {
                this.sendMessage(CQ.at(this.fromQQ) + "请先使用【#查快递】在使用快递通知");
                return;
            }
            object portV = Conf.getConfig("global.config", "port");
            int port = 10268;
            if (portV != null) {
                try {
                    port = int.Parse(portV.ToString());
                } catch (Exception e) {

                }
            }
            Dictionary<object, object> udpData = new Dictionary<object, object>();
            udpData.Add("ip", "127.0.0.1");
            udpData.Add("port", port);
            udpData.Add("group", this.fromGroup);
            udpData.Add("qq", this.fromQQ);

            string apiUrl = "https://api.acgxt.com/kuaidiNotify";
            string notifyType = "udp";
            string notifyValue = JsonConvert.SerializeObject(udpData);


            string param = String.Format("type={0}&number={1}&notify_type={2}&notify_value={3}", kdType, number, notifyType, notifyValue);

            data = Util.httpRequest(apiUrl, "POST", param);

            if (!Util.isJson(data)) {
                Log.addLog("KUAIDI_NOTIFY", "使用快递通知失败:" + data);
                this.sendMessage(CQ.at(this.fromQQ) + "使用快递通知失败!");
                return;
            }
            json = JObject.Parse(data);

            int status = int.Parse(json["status"].ToString());

            if (status == 1) {
                this.sendMessage(CQ.at(this.fromQQ) + json["msg"].ToString());
                return;
            }

            this.sendMessage(CQ.at(this.fromQQ) + "创建快递通知成功\r\n快递订单:" + number + "\r\n物流信息更变将会在本群通知您!");

        }
    }
}
