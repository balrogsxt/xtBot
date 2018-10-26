using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    /// <summary>
    /// 快递查询
    /// </summary>
    class KuaidiSelect : GroupEventApi,GroupEvents{


        public void run() {
            if (this.checkSleep(5)) return;

            string number = this.getValue();
            string kdType = String.Empty;
            string data = String.Empty;
            string getTypesApi = "https://api.acgxt.com/home/XtTools/getKuaidiTypes";
            string getInfoApi = "https://api.acgxt.com/home/XtTools/getKuaidiInfo";
            JObject json;
            bool flag = false;

            if (Util.checkEmpty(this.getValue())) {
                //验证是否存在缓存
                object cache = Conf.getConfig("global.kuaidi." + this.fromGroup, this.fromQQ.ToString());
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
            }




            if (!flag) {
                if (!Regex.IsMatch(number, @"^[0-9a-zA-Z]+$")) {
                    this.sendMessage(CQ.at(this.fromQQ) + "请输入正确的快递单号!");
                    return;
                }
                number = number.ToUpper();

                //获取订单类型

                data = Util.httpGet(string.Format("{0}?number={1}", getTypesApi, number));

                if (!Util.isJson(data)) {
                    this.sendMessage(CQ.at(this.fromQQ) + "查询快递物流失败");
                    return;
                }

                json = JObject.Parse(data);
                JArray types;
                try {
                    int status = int.Parse(json["status"].ToString());

                    if (status == 1) {
                        this.sendMessage(CQ.at(this.fromQQ) + json["msg"].ToString());
                        return;
                    }
                    if (!Util.isJsonArray(json["types"].ToString())) {
                        throw new Exception("查询快递物流异常");
                    }
                    types = JArray.Parse(json["types"].ToString());
                } catch (Exception e) {
                    Log.addLog("KUAIDI", "查询快递物流异常:" + e.Message);
                    this.sendMessage(CQ.at(this.fromQQ) + "查询快递物流异常");
                    return;
                }
                if (types.Count == 0) {
                    this.sendMessage(CQ.at(this.fromQQ) + "没有查到该订单的物流信息");
                    return;
                }
                //获取第一个快递订单进行处理
                kdType = types.First.ToString();
            }
            data = Util.httpGet(string.Format("{0}?number={1}&type={2}", getInfoApi, number, kdType), 5000);
            if (!Util.isJson(data)) {
                this.sendMessage(CQ.at(this.fromQQ) + "查询快递信息失败");
                return;
            }
            json = JObject.Parse(data);

            try {
                int status = int.Parse(json["status"].ToString());

                if (status == 1) {
                    this.sendMessage(CQ.at(this.fromQQ) + json["msg"].ToString());
                    return;
                }
                if (!Util.isJsonArray(json["data"].ToString())) {
                    throw new Exception("查询快递信息异常");
                }
                JArray list = JArray.Parse(json["data"].ToString());


                string content = CQ.at(this.fromQQ);

                for (int i = 0; i < list.Count; i++) {

                    string str = list[i]["context"].ToString();
                    string city = "";
                    if (str.Split('|').Length == 2) {
                        city = "【" + str.Split('|')[0] + "】";
                        str = str.Split('|')[1];
                    }

                    str = str.Replace("【", "[");
                    str = str.Replace("】", "]");
                    str = str.Replace("，", ",");

                    content += "\r\n" + city + list[i]["time"] + "\r\n" + str;
                }
                this.sendMessage(content);
                //储存
                Dictionary<object, object> kd = new Dictionary<object, object>();
                kd.Add("number", number);
                kd.Add("type", kdType);


                Conf.setConfig("global.kuaidi." + this.fromGroup, this.fromQQ.ToString(), JsonConvert.SerializeObject(kd));


            } catch (Exception e) {
                Log.addLog("KUAIDI", "查询快递信息异常:" + e.Message);
                this.sendMessage(CQ.at(this.fromQQ) + "查询快递信息异常");
                return;
            }


        }
        
    }
}
