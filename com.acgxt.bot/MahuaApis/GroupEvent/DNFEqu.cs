using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class DNFEqu : GroupEventApi, GroupEvents {


        public void run() {
            if (this.checkSleep(3)) return;


            string itemName = this.getValue();

            try {
                if (Util.checkEmpty(itemName)) {
                    this.sendMessage(CQ.at(this.fromQQ) + "查询的装备名称不能为空");
                    return;
                }
                //获取token
                object tokenV = Conf.getConfig("global.config", "token");
                if (tokenV == null) {
                    this.sendMessage(CQ.at(this.fromQQ) + "目前无法使用");
                    return;
                }

                Dictionary<object, object> data = new Dictionary<object, object>();
                data.Add("token", tokenV.ToString());
                data.Add("value", itemName);
                data.Add("type", "searchItem");

                UdpClient udpcSend = new UdpClient();

                byte[] res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse("10270".ToString()));
                udpcSend.Send(res, res.Length, remoteIpep);
                res = udpcSend.Receive(ref remoteIpep);

                string result = Encoding.UTF8.GetString(res);
                if (!Util.isJson(result)) {
                    this.sendMessage(CQ.at(this.fromQQ) + "查询失败");
                    return;
                }
                JObject json = JObject.Parse(result);
                int status = int.Parse(json["status"].ToString());
                if (status == 1) {
                    this.sendMessage(CQ.at(this.fromQQ) + "查询失败:" + json["msg"].ToString());
                    return;
                }
                int itemId = int.Parse(json["msg"].ToString());
                if (itemId == 0) {
                    this.sendMessage(CQ.at(this.fromQQ) + "没有找到这个装备!");
                    return;
                }
                string imgUrl = String.Format("http://bb.img.qq.com/bbcdn/dnf/equips/equimg/{0}.png", itemId);
                this.sendMessage(CQ.httpImage(imgUrl, "png"));

                udpcSend.Close();
            }catch(Exception e) {
                Log.error("查询装备系统失败:" + e.Message);
                this.sendMessage(CQ.at(this.fromQQ) + "查询失败,系统错误");
                return;
            }


        }
    }
}
