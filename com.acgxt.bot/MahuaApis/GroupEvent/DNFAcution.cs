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
    class DNFAcution : GroupEventApi, GroupEvents {



        public void run() {

            if (this.checkSleep(4)) return;



            string name = this.getValue();

            try {
                if (Util.checkEmpty(name)) {
                    this.sendMessage(CQ.at(this.fromQQ) + "查询的拍卖行商品名称不能为空");
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
                data.Add("value", name);
                data.Add("type", "searchAcution");

                UdpClient udpcSend = new UdpClient();

                byte[] res = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse("10270".ToString()));
                udpcSend.Send(res, res.Length, remoteIpep);
                res = udpcSend.Receive(ref remoteIpep);

                string result = Encoding.UTF8.GetString(res);
                udpcSend.Close();
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
                int type = int.Parse(json["msg"]["type"].ToString());
                if (type==1) {//DNF助手提供数据
                    string itemName = json["msg"]["itemName"].ToString();
                    string itemId = json["msg"]["itemId"].ToString();
                    long itemPrice = long.Parse(json["msg"]["itemPrice"].ToString());
                    string imgUrl = String.Format("http://ossweb-img.qq.com/images/bangbang/mobile/dnf/acution/item/{0}.png", itemId);
                    this.sendMessage(CQ.at(this.fromQQ) + "查询拍卖价格成功!\r\n拍卖商品名称:" + itemName + CQ.httpImage(imgUrl, "png") + "\r\n最近交易价格:" + itemPrice.ToString("###,###,###") + "金币");

                } else if (type == 2) {//第三方提供数据
                    string itemName = json["msg"]["name"].ToString();
                    string itemId = json["msg"]["id"].ToString();
                    string itemTime = json["msg"]["time"].ToString();
                    long itemPrice = long.Parse(json["msg"]["price"].ToString());
                    this.sendMessage(CQ.at(this.fromQQ) + "查询拍卖价格成功!\r\n拍卖商品名称:" + itemName +"\r\n最近交易价格:" + itemPrice.ToString("###,###,###") + "金币\r\n本数据记录时间:"+itemTime);

                }

                //sendMessage(CQ.httpImage(imgUrl, "png"));

            } catch (Exception e) {
                Log.error("查询拍卖商品失败:" + e.Message);
                this.sendMessage(CQ.at(this.fromQQ) + "查询失败,系统错误");
                return;
            }
        }


        }
    }
