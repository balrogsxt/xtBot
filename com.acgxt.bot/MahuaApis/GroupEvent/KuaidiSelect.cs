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
        private Dictionary<string, string> kdList = new Dictionary<string, string>();

        private string getKdName(string kdCode) {
            foreach (var item in this.kdList) {
                if (item.Key.Equals(kdCode)) {
                    return item.Value;
                }
            }
            return kdCode;
        }
        private void addType(string code,string name) {
            if (!this.kdList.ContainsKey(code)) {
                this.kdList.Add(code, name);
            }
        }
        private void init_type() {
            this.addType("shentong", "申通");
            this.addType("ems", "EMS");
            this.addType("shunfeng", "顺丰");
            this.addType("yuantong", "圆通");
            this.addType("zhongtong", "中通");
            this.addType("rufengda", "如风达");
            this.addType("yunda", "韵达");
            this.addType("tiantian", "天天");
            this.addType("huitongkuaidi", "百世");
            this.addType("quanfengkuaidi", "全峰");
            this.addType("debangwuliu", "德邦");
            this.addType("zhaijisong", "宅急送");
            this.addType("anxindakuaixi", "安信达");
            this.addType("huitongkuaidi", "百世快递");
            this.addType("youzhengguonei", "包裹平邮");
            this.addType("bangsongwuliu", "邦送物流");
            this.addType("dhl", "DHL快递");
            this.addType("datianwuliu", "大田物流");
            this.addType("debangwuliu", "德邦快递");
            this.addType("ems", "EMS国内");
            this.addType("emsguoji", "EMS国际");
            this.addType("ems", "E邮宝");
            this.addType("rufengda", "凡客配送");
            this.addType("guotongkuaidi", "国通快递");
            this.addType("youzhengguonei", "挂号信");
            this.addType("gongsuda", "共速达");
            this.addType("youzhengguoji", "国际小包");
            this.addType("tiandihuayu", "华宇物流");
            this.addType("jiajiwuliu", "佳吉快运");
            this.addType("jiayiwuliu", "佳怡物流");
            this.addType("canpost", "加拿大邮政");
            this.addType("kuaijiesudi", "快捷速递");
            this.addType("longbanwuliu", "龙邦速递");
            this.addType("lianbangkuaidi", "联邦快递");
            this.addType("lianhaowuliu", "联昊通");
            this.addType("ganzhongnengda", "能达速递");
            this.addType("quanyikuaidi", "全一快递");
            this.addType("quanritongkuaidi", "全日通");
            this.addType("shentong", "申通快递");
            this.addType("shunfeng", "顺丰快递");
            this.addType("suer", "速尔快递");
            this.addType("tnt", "TNT快递");
            this.addType("tiantian", "天天快递");
            this.addType("tiandihuayu", "天地华宇");
            this.addType("ups", "UPS快递");
            this.addType("youzhengguonei", "邮政包裹");
            this.addType("zhongtong", "中通快递");
            this.addType("zhongtiewuliu", "中铁快运");
            this.addType("zhaijisong", "宅急送");
            this.addType("jd","京东快递");
            this.addType("zhongyouwuliu", "中邮物流");
        }
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

            this.init_type();
            //检查是否有分隔符
            if (this.args.Length == 2) {
                number = this.args[0];
                kdType = this.args[1];
                if (kdType.Trim().Length==0) {
                    this.sendMessage(CQ.at(this.fromQQ) + "请输入正确的快递类型!");
                    return;
                }
                //中文模糊检查
                foreach (var item in this.kdList) {
                    if (item.Value.ToLower().Contains(kdType.ToLower())) {
                        kdType = item.Key;
                        //跳出
                        break;
                    }
                }

                //this.sendMessage("开始查询:"+kdType);
                flag = true;
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


                string content = CQ.at(this.fromQQ)+ "\r\n单号:" + number + "\r\n物流:" + this.getKdName(kdType) +"";

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
