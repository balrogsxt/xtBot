using com.acgxt.bot.Core;
using com.acgxt.bot.MahuaApis.Module;
using com.acgxt.bot.Utils;
using com.acgxt.bot.Utils.XtException;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newbe.Mahua.Logging;
using Newbe.Mahua.MahuaEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageEvent : IGroupMessageReceivedMahuaEvent {
        public static int msgid = 0;
        private readonly IMahuaApi api;
        private int subType;
        private long fromGroup;
        private long fromQQ;
        private string fromAnonymous;
        private string msg;
        private int font;
        private string key = null;
        private long debugRootQQ = 2289453456;
        //允许操作的管理员
        private List<long> rootQQ = new List<long>();
        //允许请求的群
        private List<long> allSendGroup = new List<long>();
        //屏蔽的qq号
        private List<long> NoReplayQQ = new List<long>();

        public void ProcessGroupMessage(GroupMessageReceivedContext context) {
            try {
                
                //初始化
                this.fromQQ = long.Parse(context.FromQq);
                this.fromGroup = long.Parse(context.FromGroup);
                this.msg = context.Message;
                GroupMessageEvent.msgid += 1;
                Data.api = this.api;

                //载入配置项
                this.loadConfig();
                //验证允许群和禁止qq
                if (this.initCheck() == false) return;
                //直接检测是否为空
                if (this.checkEmpty(this.msg)) return;
                

                //首次处理广告词  这里为true后将不再处理后面的数据
                if (this.checkAds(true)){
                    return;
                }

                try {
                    Dictionary<string, object> modules = new Dictionary<string, object>();

                    modules.Add("#查快递","KuaidiSelect");
                    modules.Add("#快递通知", "KuaidiNotify");
                    modules.Add("#二维码", "QrCode");
                    modules.Add("#icon", "Favicon");
                    modules.Add("#查装备", "DNFEqu");
                    modules.Add("#查拍卖", "DNFAcution");
                    modules.Add("#查", "Cha");
                    modules.Add("查", "Cha");

                    foreach (var item in modules) {
                        try {
                            string commandName = item.Key;
                            object moduleName = item.Value;
                            if (!this.isValidate(commandName)) {
                                continue;
                            }
                            Type module;
                            string classPath;
                            if (item.Value is Type) {
                                module = (Type)item.Value;
                                classPath = String.Format("com.acgxt.bot.MahuaApis.GroupEvent.{0}", module.Name);
                            } else if (item.Value is String) {
                                classPath = String.Format("com.acgxt.bot.MahuaApis.GroupEvent.{0}", moduleName);
                                module = Type.GetType(classPath);
                            } else {
                                continue;
                            }



                            try {
                                object obj = module.Assembly.CreateInstance(classPath);
                                MethodInfo method;
                                //init 
                                method = module.GetMethod("__init");
                                object[] baseData = { item.Key, this.getValue(), this.fromGroup, this.fromQQ };
                                method.Invoke(obj, baseData);

                                method = module.GetMethod("run");
                                object[] runData = { };
                                method.Invoke(obj, runData);
                            }catch(NoException e) {
                                CQAPI.xtAddLog(LogType.status.DEBUG,"模块信息",e.Message);
                            }

                            return;
                        }catch(Exception e) {
                            this.sendRootMessage("使用模块:"+item.Key+"\r\n发生异常:" + e.Message+"\r\n"+e.ToString());
                            return;
                        }
                    }
                }catch(Exception e){
                    this.sendRootMessage("发送异常:" + e.Message+"\r\n"+e.ToString());
                    return;
                }




                if (this.msg.Contains("帮助")|| this.msg.Contains("help")) {
                    string msg = this.atself()+"帮助列表";
                    string[] helps = {
                        "发送 \"#查快递\" 加订单号可查询快递物流信息",
                        "发送 \"#快递通知\" 可将上次查询的快递状态实时通知",
                        "发送 \"#二维码\" 获取输入信息的二维码图片",
                        "发送 \"#icon\" 获取输入的Url地址Favicon图片",
                        "发送 \"#查装备\" 加装备名称可查询DNF装备信息",
                        "发送 \"#查拍卖\" 加拍卖商品名称可查询最近交易价格",
                        "",
                        "命令使用方法例如:#查快递123456",
                        "以上命令为可以使用的功能(不包含管理员命令)",
                        "机器人源码地址:https://git.io/fxrLl",
                        "但不代表最新版本,反馈请联系i@acgxt.com"
                    };
                    for (int i=0;i<helps.Length;i++) {
                        msg += "\r\n"+helps[i];
                    }
                    this.sendMessage(msg);
                    return;
                }
                if (this.isValidate("#重启")) {
                    this.restart();
                    return;
                }

                //base
                if (this.isValidate("setVar=")) {
                    this.setVar();
                    return;
                } else
                if (this.isValidate("send=")) {
                    this.sendMessage(this.getValue());
                    return;
                } else
                if (this.isValidate("#读图片")) {
                        string url = CQ.getImageUrl(this.getValue());
                        this.sendMessage(url);
                    
                    return;
                }else

                if (this.isValidate("#复读姬")) {
                    this.groupNumEvent("复读姬", "reReplySize");
                    return;
                } else

                if (this.isValidate("#复读")) {

                    this.groupJilvEvent("复读", "fudu");
                    return;
                }else

                if (this.isValidate("#盗图")) {
                    this.groupJilvEvent("盗图", "daotu");
                    return;
                } else
                if (this.isValidate("#回复")) {
                    this.groupJilvEvent("关键字回复", "reply");
                    return;
                } else
                if (this.isValidate("#聊天储存")) {
                    this.groupToggleEvent("聊天云储存", "savemsg");
                    return;
                } else
                if (this.isValidate("#图片审核")) {
                    this.groupToggleEvent("图片审核", "imagecheck");
                    return;
                } else
                if (this.isValidate("#文字审核")) {
                    this.groupToggleEvent("文字审核", "adscheck");
                    return;
                } else




                //管理员命令
                if (this.isValidate("#添加管理员=")) {
                    this.addAdmin();
                    return;
                } else if (this.isValidate("#添加群=")) {
                    this.addGroup();
                    return;
                } else if (this.isValidate("#屏蔽=")) {
                    this.addnoreply();
                    return;
                } else if (this.isValidate("#info")) {//查看信息
                    this.test();
                    return;
                } else if (this.isValidate("#添加")) {//全局添加
                    this.groupReplayAdd(true);
                    return;
                } else if (this.isValidate("#删除")) {//全局删除
                    this.groupReplayDel(true);
                    return;
                } 
   
                
                
                else if (this.isValidate("添加")) {
                    this.groupReplayAdd(false);
                    return;
                } else if (this.isValidate("删除")) {
                    this.groupReplayDel(false);
                    return;
                }
                this.groupReplayCheck();


                this.checkImage();

                ////请求储存消息
                //this.saveMsg();

            } catch(Exception e) {
                Log.addLog("SYSTEM_ERROR", fromGroup.ToString(), fromQQ.ToString(),"发生异常:"+ e.Message + "=>" + e.StackTrace);
                this.sendRootMessage(String.Format("在群{0}中发生异常\r\n{1},\r\n详情查询日志文件处理 ",fromGroup, e.Message));
            }
        }
        /// <summary>
        /// 频率限制
        /// </summary>
        /// <param name="s"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private bool ignore(string key,int s,int max) {
            //10分钟内限定回复3次
            //int max = 2;//3次重置,更新时间戳
            int num = 0;
            long liveTime = Util.getTime();
            long time = Util.getTime();
            object numObj = Conf.getConfig("global.group." + fromGroup + ".ignore1", key + "-" + fromQQ);
            if (numObj != null) {
                if (numObj.ToString().Contains("-")) {
                    //0=>次数
                    //1=>时间
                    string[] arr = numObj.ToString().Split('-');
                    try {
                        num = int.Parse(arr[0]);
                    } catch (Exception e) { }

                    try {
                        time = long.Parse(arr[1]);
                    } catch (Exception e) { }
                }
            }



            //判断储存的时间是否大于现在,大于则不执行
            int ac = CQAPI.getAuthCode();

            if (time > liveTime) {
                CQAPI.xtAddLog(ac, LogType.status.INFO, "普通用户忽略", "存在多次使用非管理员命令【" + key + "】目前还在忽略时间内");
                return true;
            }
            //自增1
            num += 1;


            //get success
            if (num >= max) {
                //重置次数
                //增加10分钟的屏蔽

                long closeTime = liveTime += 60 * 1;

                string data = "0-" + closeTime;

                Conf.setConfig("global.group." + fromGroup + ".ignore1", key + "-" + fromQQ, data);
                //不执行后面操作
                CQAPI.xtAddLog(ac, LogType.status.INFO, "普通用户忽略", "存多次使用命令【" + key + "】当前开始进行1分钟忽略");
                return true;
            } else {
                string data = num + "-" + time;
                Conf.setConfig("global.group." + fromGroup + ".ignore1", key + "-" + fromQQ, data);
                CQAPI.xtAddLog(ac, LogType.status.INFO, "普通用户忽略", "使用多次命令【" + key + "】当前第" + num + "次," + max + "次后将进行忽略1分钟!");

            }
            return false;

        }
        public GroupMessageEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }
        //检查是否为空
        private bool checkEmpty(string text) {
            try {
                //基础检查
                if (text.Length == 0 || text == "") {
                    return true;
                }
                if (text.Trim().Length == 0 || text.Trim() == "") {
                    return true;
                }
                //高级检查
                if (text.Replace(" ", "").Trim() == "" || text.Replace("\r\n", "").Trim() == "") {
                    return true;
                }

                return false;
            } catch (Exception e) {
                return true;
            }
        }
        private void sendRootMessage(object data) {
            if (this.checkEmpty(data.ToString())) return;
            this.api.SendPrivateMessage(this.debugRootQQ.ToString(), data.ToString());
        }
        //发送当前群消息
        private void sendMessage(object data) {
            this.sendMessage(data,"未知来源");
        }
        private void sendMessage(object data,string og) {
            if (this.checkEmpty(data.ToString())) {
                return;
            }
            CQAPI.xtAddLog(LogType.status.DEBUG, "群聊消息", "[来自"+ og + "发送的数据]");
            this.api.SendGroupMessage(this.fromGroup.ToString(), CQ.CQString(data.ToString()));
        }
        private void sendMessage(object data,bool isReplace) {
            if (this.checkEmpty(data.ToString())) {
                return;
            }
            string message = data.ToString();
            if (isReplace) {
                message = CQ.CQString(message);
            }
            this.api.SendGroupMessage(this.fromGroup.ToString(), message);
        }
        //清除空格
        private string clearEmpty(string text) {
            text = text.Replace(" ", "").Trim();
            text = text.Replace("\r\n", "").Trim();
            return text;
        }
        //获取管理员,允许群,
        private void loadConfig() {

            //手动设置允许群
            this.allSendGroup.Add(550505327);



            //以下是配置文件输出
            JArray jarr;
            //载入管理员
            object adminData = Conf.getConfig("global.config", "admin");
            try {
                jarr = (JArray)adminData;
                for (int i = 0; i < jarr.Count; i++) {
                    try {
                        long v = (long)jarr[i];
                        if (!this.rootQQ.Contains(v)) {

                            this.rootQQ.Add((long)jarr[i]);
                        }
                    } catch (Exception e) {

                    }
                }
            } catch (Exception e) {

            }

            //载入允许群
            object allowgroup = Conf.getConfig("global.config", "group");
            try {
                jarr = (JArray)allowgroup;
                for (int i = 0; i < jarr.Count; i++) {
                    try {
                        long v = (long)jarr[i];
                        if (!this.allSendGroup.Contains(v)) {
                            this.allSendGroup.Add((long)jarr[i]);
                        }
                    } catch (Exception e) {

                    }
                }
            } catch (Exception e) {

            }


            //载入屏蔽群员
            object noreplyqq = Conf.getConfig("global.config", "noreplyqq");
            try {
                jarr = (JArray)noreplyqq;
                for (int i = 0; i < jarr.Count; i++) {
                    try {
                        long v = (long)jarr[i];
                        if (!this.NoReplayQQ.Contains(v)) {
                            this.NoReplayQQ.Add((long)jarr[i]);
                        }

                    } catch (Exception e) {

                    }
                }
            } catch (Exception e) {

            }




        }
        //获取值
        private string getValue() {
            if (this.key != null && this.key != "") return this.msg.Substring(this.key.Length).Trim(); return null;
        }
        //验证值
        private bool isValidate(string keyword) {
            int length = keyword.Length;
            if (keyword.Length > msg.Length) {
                length = msg.Length;
            }
            if (this.msg.Substring(0, length) == keyword) {
                this.key = keyword;
                return true;
            } else {
                return false;
            }
        }
        //检查是否是管理员
        private bool initCheckAdmin() {
            bool isNoAllow = false;
            for (int i = 0; i < this.rootQQ.Count; i++) {
                if (this.rootQQ[i] == this.fromQQ) {
                    isNoAllow = true;
                    break;
                }
            }
            if (isNoAllow) {
                return true;
            }
            return false;
        }
        //初始化检查允许群QQ
        private bool initCheck() {
            //屏蔽指定qq号发送的消息
            bool isNoAllow = false;
            for (int i = 0; i < this.NoReplayQQ.Count; i++) {
                if (this.NoReplayQQ[i] == this.fromQQ) {
                    isNoAllow = true;
                    break;
                }
            }
            if (isNoAllow) {
                return false;
            }

            bool isAllow = false;
            for (int i = 0; i < this.allSendGroup.Count; i++) {
                if (this.allSendGroup[i] == this.fromGroup) {
                    isAllow = true;
                    break;
                }
            }
            if (!isAllow) {
                return false;
            }
            return true;

        }
        //获取测试信息
        private void test() {
            string t1 = "";
            for (int i = 0; i < this.rootQQ.Count; i++) {
                t1 += "\r\n" + this.rootQQ[i];
            }
            this.sendMessage("管理员列表:" + t1,"test");
            string t2 = "";
            for (int i = 0; i < this.allSendGroup.Count; i++) {
                t2 += "\r\n" + this.allSendGroup[i];
            }
            this.sendMessage("允许群列表:" + t2, "test");
            string t3 = "";
            for (int i = 0; i < this.NoReplayQQ.Count; i++) {
                t3 += "\r\n" + this.NoReplayQQ[i];
            }
            this.sendMessage("屏蔽人列表:" + t3, "test");
        }
        //AT当前用户
        private string atself() {
            return CQ.at(this.fromQQ);
        }
        private void restart() {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("cqp_restart") == false) {
                    this.sendMessage(this.atself() + "您没有权限操作机器人重启功能!","restart");
                }
                return;
            }


            CQRestart cqp = new CQRestart();
            cqp.onLogger += (s) => {
                MahuaRobotManager.Instance.CreateSession().MahuaApi.SendGroupMessage(fromGroup.ToString(), CQ.at(fromQQ) + s);
            };
            cqp.run(this.fromGroup.ToString(), this.fromQQ.ToString());
        }
        private void setVar() {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("setVar") == false) {
                    this.sendMessage(this.atself() + "您没有权限操作变量!", "setvar");
                }
                return;
            }

            string val = this.getValue();
            if (val == null) return;
            string[] txt = val.Split('-');
            if (2 > txt.Length) {
                this.sendMessage(CQ.at(this.fromQQ) + "请输入变量和变量值!命令请查看帮助", "setvar");
                return;
            }
            string key = txt[0];
            string value = val.Substring(key.Length + 1);
            if (this.clearEmpty(key) == ":") {
                this.sendMessage("我可去.....");
                return;
            }

            if (this.checkEmpty(key) || this.checkEmpty(value)) {
                this.sendMessage(CQ.at(this.fromQQ) + "变量名称或变量值不能为空!命令请查看帮助");
                return;
            }
            try {
                Conf.setVar(key, value);
                this.sendMessage(this.atself() + "设置变量成功\r\n"+key+"\r\n" + value,false);

            } catch (Exception e) {
                this.sendMessage(this.atself()+"设置发生异常:"+e.Message);
            }







        }
        private bool ignoreAdmin(string key) {
            //10分钟内限定回复3次
            int max = 2;//3次重置,更新时间戳
            int num = 0;
            long liveTime = Util.getTime();
            long time = Util.getTime();
            object numObj = Conf.getConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ);
            if (numObj!=null) {
                if (numObj.ToString().Contains("-")) {
                    //0=>次数
                    //1=>时间
                    string[] arr = numObj.ToString().Split('-');
                    try {
                        num = int.Parse(arr[0]);
                    } catch (Exception e) { }

                    try {
                        time = long.Parse(arr[1]);
                    } catch (Exception e) { }


                }
            }



            //判断储存的时间是否大于现在,大于则不执行
            int ac = CQAPI.getAuthCode();

            if (time>liveTime) {
                CQAPI.xtAddLog(ac, LogType.status.INFO, "非管理员忽略","存在多次使用非管理员命令【"+key+"】目前还在忽略时间内");
                return true;
            }
            //自增1
            num += 1;


            //get success
            if (num>=max) {
                //重置次数
                //增加10分钟的屏蔽

                long closeTime = liveTime += 60 * 10;

                string data = "0-" + closeTime;

                Conf.setConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ,data);
                //不执行后面操作
                CQAPI.xtAddLog(ac, LogType.status.INFO, "管理员忽略","存在多次使用非管理员命令【"+key+"】当前开始进行10分钟忽略");
                return true;
            } else {
                string data = num+"-" + time;
                Conf.setConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ, data);
                CQAPI.xtAddLog(ac, LogType.status.INFO, "管理员忽略", "使用非管理员命令【" + key + "】当前第"+num+"次,"+max+"次后将进行忽略10分钟!");

            }
            return false;

        }
        //开启Or关闭
        private void groupToggleEvent(string name, string confKey) {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("groupToggleEvent_" + confKey) == false) {
                    this.sendMessage(this.atself() + String.Format("目前不允许非管理员修改{0}!", name));
                }
                return;
            }
            int toggle = 0;
            try {
                if (this.getValue().Equals("开启")) {
                    toggle = 1;
                } else if (this.getValue().Equals("关闭")) {
                    toggle = 0;
                } else {
                    this.sendMessage(this.atself() + String.Format("请输入正确的命令【开启/关闭】", name));
                    return;
                }

            } catch (Exception e) { }


            Conf.setConfig("global.group." + fromGroup.ToString(), confKey, toggle);
            if (toggle == 0) {
                this.sendMessage(this.atself() + String.Format("本群{0}功能已关闭", name));
            } else {
                this.sendMessage(this.atself() + String.Format("本群{0}功能已开启", name, toggle));
            }
            return;


        }


        //几率开启
        private void groupJilvEvent(string name, string confKey) {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("groupJilvEvent_" + confKey) == false) {
                    this.sendMessage(this.atself() + String.Format("目前不允许非管理员修改{0}几率!", name));
                }
                return;
            }
            try {
                double data = double.Parse(this.getValue());
                if (this.getValue().Contains(".")) {
                    if (this.getValue().Split('.')[1].Length > 3) {
                        this.sendMessage(this.atself() + String.Format("{0}几率最多保留3为小数!", name));
                        return;
                    }
                }
                if (data >= 100) data = 100;
                Conf.setConfig("global.group." + fromGroup.ToString(), confKey, data);
                if (data == 0) {
                    this.sendMessage(this.atself() + String.Format("本群{0}功能已关闭", name));
                } else {
                    this.sendMessage(this.atself() + String.Format("设置{0}几率成功,当前群{0}几率为{1}%", name, data));
                }
            } catch (Exception e) {
                this.sendMessage(this.atself() + "请输入正确的数字,支持小数点!");
            }
            return;
        }
        //数字修改
        private void groupNumEvent(string name, string confKey) {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("groupNumEvent_" + confKey) == false) {
                    this.sendMessage(this.atself() + String.Format("目前不允许非管理员修改{0}值!", name));
                }
                return;
            }
            try {
                int data = int.Parse(this.getValue());

                if (0 > data) data = 0;
                Conf.setConfig("global.group." + fromGroup.ToString(), confKey, data);
                if (data == 0) {
                    this.sendMessage(this.atself() + String.Format("本群{0}功能已关闭", name));
                } else {
                    this.sendMessage(this.atself() + String.Format("设置{0}成功,当前群{0}值为{1}", name, data));
                }
            } catch (Exception e) {
                this.sendMessage(this.atself() + "请输入正确的数字!");
            }
            return;
        }
        //将聊天数据储存到腾讯云数据库
        private void saveMsg()
        {

            int savemsg = 1;
            object savemsgCheck = Conf.getConfig("global.group." + fromGroup.ToString(), "savemsg");
            if (savemsgCheck != null) {
                try {
                    savemsg = int.Parse(savemsgCheck.ToString());
                } catch (Exception e) { }
            }

            if (savemsg==1) {
                //储存消息到数据库
                //获取token
                object tokenV = Conf.getConfig("global.config", "token");
                if (tokenV == null) {
                    Log.addLog("SAVE_MSG","验证失败:token服务端未设定,无法使用云储存");
                    return;
                }


                string token = tokenV.ToString();


                string apiUrl = "https://api.acgxt.com/cqp/addmsg?token="+ token;
                string param = String.Format("message={0}&msgid={1}&qq={2}&group={3}", this.msg,0, this.fromQQ, this.fromGroup);
                string result = Util.httpRequest(apiUrl, "POST", param, "UTF-8", 10000);
            }
        }
        //验证图片是否违规
        private void checkImage() {

            int toggle = 1;
            object toggleObj = Conf.getConfig("global.group." + fromGroup.ToString(), "imagecheck");
            if (toggleObj != null) {
                try {
                    toggle = int.Parse(toggleObj.ToString());
                } catch (Exception e) { }
            }
            if (toggle==0) {//已关闭
                this.saveMsg();
                return;
            }


            //目前只支持单张图片验证发送,不支持gif
            var reg = @"^\[CQ:image,file=([A-Z0-9]+).(png|jpg|bmp|jpeg)\]$";



            MatchCollection mc = Regex.Matches(msg, reg);
            string fileName = null;
            foreach (Match m in mc) {
                fileName = m.Groups[1] + "." + m.Groups[2];
            }
            if (fileName == null) {

                var gifreg = @"\[CQ:image,file=([A-Z0-9]+).gif\]";
                //检查是否存在gif
                mc = Regex.Matches(msg, reg);
                foreach (Match m in mc) {
                    return;
                }




                this.saveMsg();//储存到云数据
                return;
            }

            long msgid = Data.getMsgid(msg, fromGroup.ToString());

            Task.Factory.StartNew(() => {
                using (var robotSession = MahuaRobotManager.Instance.CreateSession()) {
                    var api = robotSession.MahuaApi;

                    //临时验证
                    object cache = Conf.getConfig("global.imgads", msg);
                    if (cache!=null) {
                        try {
                            //正常图片
                            int.Parse(cache.ToString());
                            saveMsg();//储存到云数据

                        } catch (Exception e) {
                            //不合格图片
                            string reasonValue = cache.ToString();
                            int start = reasonValue.Length - 1;
                            if (reasonValue.Substring(start,1) =="、") {
                                reasonValue = reasonValue.Substring(0, reasonValue.Length-1);
                            }
                            //IMAGE_CHECK_CACHE
                            Log.addLog("IMAGE_CHECK_CACHE", fromGroup.ToString(),fromQQ.ToString(),"拦截不合格图片【"+msg+"】内容存在【"+ reasonValue + "】");
                            string reason = CQ.at(fromQQ)+"乃发送的图片含有【" + reasonValue + "】!";
                            TimeSpan ts = new TimeSpan(0, 1, 0);//禁言1分钟
                            api.BanGroupMember(fromGroup.ToString(),fromQQ.ToString(), ts);
                            int flag = CQ.deleteMessage(msgid);//撤回消息
                            api.SendGroupMessage(fromGroup.ToString(), reason);
                            Log.addLog("BAN_SEND_MESSAGE", fromGroup.ToString(), fromQQ.ToString(), "违规发布不合格图片,禁言1分钟,图片存在【" + reasonValue + "】");

                        }
                        return;

        

                    }







                    string data = Util.downloadImageFile(msg);
                    //api.SendGroupMessage(fromGroup.ToString(), data);
                    var image = File.ReadAllBytes(data);

                    try {
                        JObject result = Conf.ic.UserDefined(image);

                        if (result.Property("conclusionType")==null) {
                            return;//发生错误
                        }
                        int type = int.Parse(result["conclusionType"].ToString());
                        //api.SendGroupMessage(fromGroup.ToString(), result.ToString());

                        switch (type) {
                            case 1:
                                //图片正常
                                Conf.setConfig("global.imgads",msg, 0);
                                Log.addLog("IMAGE_CHECK_FIRST", fromGroup.ToString(), fromQQ.ToString(), "图片【" + msg + "】图片正常");
                                break;
                            case 2://不合格的图片

                                //遍历不合格的理由

                                JArray list = (JArray)result["data"];
                                string reason = "";
                                string otherReason = "";
                                bool isE = false;
                                for (int i=0;i<list.Count;i++) {
                                    JObject v = (JObject)list[i];

                                    /**
                                     * 1：色情、2：性感、3：暴恐、4:恶心、5：水印码、6：二维码、7：条形码、8：政治人物、9：敏感词、10：自定义敏感词
                                     **/
                                    int typeCode = int.Parse(v["type"].ToString());
                                    int[] typeE = { 1, 2};

                                    for (int k=0;k<typeE.Length;k++) {
                                        string r = v["msg"].ToString();
                                        if (typeE[k]==typeCode) {

                                            string c = "存在";
                                            if (r.Substring(0,c.Length)=="存在") {
                                                r = r.Substring(c.Length);
                                            }
                                            reason += r+"、";
                                            isE = true;
                                            break;
                                        }
                                        otherReason += r + "、";
                                    }
                                }
                                if (isE) {
                                    string reasonValue = reason.ToString();
                                    int start = reasonValue.Length - 1;
                                    if (reasonValue.Substring(start, 1) == "、") {
                                        reasonValue = reasonValue.Substring(0, reasonValue.Length - 1);
                                    }

                                    Log.addLog("IMAGE_CHECK_FIRST", fromGroup.ToString(), fromQQ.ToString(), "拦截不合格图片【" + msg + "】内容存在【" + reasonValue + "】");

                                    string vv = CQ.at(fromQQ)+"乃发送的图片含有【" + reasonValue + "】!";
                                    Conf.setConfig("global.imgads", msg,reason);
                                    //撤回该消息
                                    TimeSpan ts = new TimeSpan(0, 1, 0);//禁言1分钟
                                    api.BanGroupMember(fromGroup.ToString(), fromQQ.ToString(), ts);
                                    int flag = CQ.deleteMessage(msgid);//撤回消息
                                    api.SendGroupMessage(fromGroup.ToString(), vv);
                                    Log.addLog("BAN_SEND_MESSAGE", fromGroup.ToString(),fromQQ.ToString(),"违规发布不合格图片,禁言1分钟,图片存在【"+ reasonValue + "】");
                                } else {
                                    //其他
                                    Conf.setConfig("global.imgads", msg,4);
                                    Log.addLog("IMAGE_CHECK_FIRST", fromGroup.ToString(), fromQQ.ToString(), "图片【" + msg + "】存在其他不符合规格元素【"+ otherReason + "】");

                                }





                                break;
                            case 3://疑似
                                Conf.setConfig("global.imgads", msg, 2);
                                Log.addLog("IMAGE_CHECK_FIRST", fromGroup.ToString(), fromQQ.ToString(), "图片【" + msg + "】疑似包含不合格元素");

                                break;
                            case 4://失败
                                Conf.setConfig("global.imgads", msg, 3);
                                Log.addLog("IMAGE_CHECK_FIRST", fromGroup.ToString(), fromQQ.ToString(), "图片【" + msg + "】审核失败");

                                break;
                            default:return;
                        }
                        if (type==1) {//只有正常图片才可以储存
                            saveMsg();//储存到云数据
                        }




                    } catch (Exception e) {
                        int ac = CQAPI.getAuthCode();
                                //api.SendGroupMessage(fromGroup.ToString(), e.StackTrace);
                        CQAPI.xtAddLog(ac, LogType.status.INFO, "检测图片", "发生错误:" + e.Message+"->"+e.StackTrace);
                    }
                    //var result = Conf.ap.Detect(image);


                }
            });
            //string url = CQ.getImageUrl(this.getValue());
        }

        private bool checkAds(bool isCustomCheck)
        {
            if (this.checkEmpty(this.msg))
            {
                return false;

            }
            
            try
            {

                //this.sendMessage("ok");

                int toggle = 1;
                object toggleObj = Conf.getConfig("global.group." + fromGroup.ToString(), "adscheck");
                if (toggleObj != null) {
                    try {
                        toggle = int.Parse(toggleObj.ToString());
                    } catch (Exception e) { }
                }
                if (toggle == 0) {//已关闭
                    isCustomCheck = false;
                }

                //处理自定义广告
                if (isCustomCheck)
                {
                    JArray jarr;
                    //载入敏感文字
                    List<string> adsKey = new List<string>();
                    //直接撤回的关键词
                    try {
                        object allowgroup = Conf.getConfig("global.ads", "ban1");
                        //if (allowgroup==null) {
                        //    this.sendMessage("nul");
                        //}
                        jarr = (JArray)allowgroup;
                        for (int i = 0; i < jarr.Count; i++) {
                            try {
                                adsKey.Add(jarr[i].ToString());
                            } catch (Exception e) {
                            }
                        }
                    } catch (Exception e) {

                    }
                    for (int i = 0; i < adsKey.Count; i++) {
                        string k = adsKey[i].ToString();
                        if (
                            //k.Contains(this.msg) || 
                            this.msg.Contains(k)) {
                            this.sendMessage(this.atself() + "请文明发言!");
                            //撤回该消息
                            Log.addLog("DELETE_MESSAGE", fromGroup.ToString(),fromQQ.ToString(),"敏感词撤回【"+ this.msg + "】");
                            this.deleteLiveMessage();
                            return true;
                        }
                    }
                    adsKey.Clear();//清空
                    //直接撤回并且禁言的关键词
                    try {
                        object allowgroup = Conf.getConfig("global.ads", "ban2");
                        jarr = (JArray)allowgroup;
                        for (int i = 0; i < jarr.Count; i++) {
                            try {
                                adsKey.Add(jarr[i].ToString());
                            } catch (Exception e) { }
                        }
                    } catch (Exception e) {

                    }
                    for (int i = 0; i < adsKey.Count; i++) {
                        string k = adsKey[i].ToString();
                        if (
                            //k.Contains(this.msg) || 
                            this.msg.Contains(k)) {
                            this.sendMessage(this.atself() + "违规发布广告!");
                            //撤回该消息
                            TimeSpan ts = new TimeSpan(0,1,0);//禁言1分钟
                            api.BanGroupMember(this.fromGroup.ToString(), this.fromQQ.ToString(), ts);
                            this.deleteLiveMessage();
                            Log.addLog("BAN_SEDN_MESSAGE", fromGroup.ToString(),fromQQ.ToString(),"违规发布广告禁言警告【"+ this.msg + "】");
                            return true;
                        }
                    }
                    adsKey.Clear();//清空

                    //直接撤回并踢出群的关键词
                    try {
                        object allowgroup = Conf.getConfig("global.ads", "ban3");
                        jarr = (JArray)allowgroup;
                        for (int i = 0; i < jarr.Count; i++) {
                            try {
                                adsKey.Add(jarr[i].ToString());
                            } catch (Exception e) { }
                        }
                    } catch (Exception e) {

                    }
                    for (int i = 0; i < adsKey.Count; i++) {
                        string k = adsKey[i].ToString();
                        if (
                            //k.Contains(this.msg) || 
                            this.msg.Contains(k)) {
                            //撤回该消息


                            int self = getGroupUserIdLevel(api.GetLoginQq());
                            int other = getGroupUserIdLevel(fromQQ);
                            this.deleteLiveMessage();


                            string msg = "";
                            if (
                                true
                                //self > other
                                ) {//权限大于发言者即可操作权限操作
     
                                object obj = Conf.getConfig("global.adskick." + fromGroup, fromQQ.ToString());
                                int warning = 2;
                                int num = 1;
                                if (obj != null) {
                                    int cacheNum = 1;
                                    try {
                                        cacheNum = int.Parse(obj.ToString());
                                    } catch (Exception e) {}
                                        
                                    num = cacheNum+1;
                                }
                                Conf.setConfig("global.adskick." + fromGroup, fromQQ.ToString(),num);
                                string msg2 = "";
                                //if (other!=0) {//身份不明确,获取失败
                                    msg = String.Format("当前警告第{0}次,第{1}次后将踢出本群", num, warning);
                                    msg2 = "警告已达最高次数,已踢出本群!";
                                //}



                                //验证次数
                                if (num>= warning) {
                                    this.sendMessage(this.atself() + "严重违规发布广告!"+msg2);
                                    //踢出该群
                                    api.KickGroupMember(this.fromGroup.ToString(), this.fromQQ.ToString(), false);
                                } else {
                                    this.sendMessage(this.atself() + "严重违规发布广告!"+msg);

                                }

                            } else {
                                this.sendMessage(this.atself() + "严重违规发布广告!");
                            }
                            Log.addLog("KICK_GROUP", fromGroup.ToString(),fromQQ.ToString(),"违规发布广告踢出该群【"+ this.msg + "】");
                            return true;
                        }
                    }
                    adsKey.Clear();//清空
                }
            }
            catch (Exception e)
            {
                Log.addLog("ADS_ERROR", fromGroup.ToString(), fromQQ.ToString(), "检测广告发生异常:" + e.Message+":"+e.Data+":"+e.StackTrace);
                return false;
            }
            return false;
        }


        private int getGroupUserIdLevel(object qq) {
            try {
                GroupMemberInfo info = api.GetGroupMemberInfo(fromGroup.ToString(), qq.ToString());
                Log.addLog("GET_USER_INFO", fromGroup.ToString(), qq.ToString(), "获取成员身份:" + info.ToString());
                switch (info.Authority) {
                    case GroupMemberAuthority.Leader: return 3;
                    case GroupMemberAuthority.Manager: return 2;
                    case GroupMemberAuthority.Normal: return 1;
                    case GroupMemberAuthority.Unknown: return 0;
                    default: return 0;
                }
            } catch(Exception e) {
                Log.addLog("GET_USER_INFO", fromGroup.ToString(), fromQQ.ToString(), "获取成员信息失败:" + e.Message);
                return 0;
            }
            
        }


        //撤回当前消息
        private void deleteLiveMessage(){
            try {
                long msgid = Data.getMsgid(this.msg, this.fromGroup.ToString());
                int flag = CQ.deleteMessage(msgid);

            } catch (Exception e) {
                this.sendMessage(e.Message);
            }
        }
        //回复检查
        private void groupReplayCheck() {




            //优先处理复读机
            //判定条件=3条重复数据则自动跟一条
            //复读姬默认关闭
            //int fdToggle = 0;
            //object fdToggleObj = Conf.getConfig("global.group." + fromGroup.ToString(), "reReply");//是否开启复读姬检测
            //if (fdToggleObj != null) {
            //    try {
            //        fdToggle = int.Parse(fdToggleObj.ToString());
            //    } catch (Exception e) { }
            //}


            int maxsize = 3;//默认最大累积3次检测
            object fdObj = Conf.getConfig("global.group." + fromGroup.ToString(), "reReplySize");//复读姬检测次数
            if (fdObj != null) {
                try {
                    maxsize = int.Parse(fdObj.ToString());
                } catch (Exception e) { }
            }
            if (maxsize != 0) {//不为0则为开启状态
                //获取配置文件中的复读次数
                
                GroupData.maxSize = maxsize;



                GroupData.addMessage(this.fromGroup,msgid, this.fromQQ, this.msg);
                try {
                    List<long> qqs = GroupData.checkReply(this.fromGroup, this.msg);

                    if (qqs != null) {
                        //reply 
                        GroupMemberInfo info = null;
                        Random r = new Random();
                        long mbgQQ = qqs[r.Next(0, qqs.Count)];
                        try {
                            info = api.GetGroupMemberInfo(fromGroup.ToString(), api.GetLoginQq());
                            //获取成功
                            if (info.Authority == GroupMemberAuthority.Leader || info.Authority == GroupMemberAuthority.Manager) {
                                this.sendMessage("发现大量复读姬出没!\r\n下面我要选择一名复读姬来禁言,到底是哪位小朋友这么幸运呢?");
                                this.sendMessage(CQ.at(mbgQQ));

                            } else {
                                this.sendMessage(msg,"复读姬跟读");
                            }
                        } catch (Exception e) {
                            //获取身份失败,默认回复
                            Log.addLog("GET_GROUP_USER", fromGroup.ToString(), fromQQ.ToString(), "获取群员信息失败:" + e.Message);
                            this.sendMessage(msg,"复读姬获取群员身份失败");
                        }


                        int banS = 1;
                        try {
                            banS = int.Parse(Conf.getVar("复读姬禁言"));
                        }catch(Exception e) {

                        }


                        //禁言
                        TimeSpan ts = new TimeSpan(0, 0, banS);
                        this.api.BanGroupMember(this.fromGroup.ToString(), mbgQQ.ToString(), ts);
                        return;
                    } else {

                    }

                } catch(ReReplyException e) {
                    this.sendMessage(e.getMsg(),"复读姬异常");
                }catch(Exception e) {
                    this.sendRootMessage(e.Message);
                }
                
            }



            object value = Conf.getConfig("reply.group." + this.fromGroup, null);
            if (value == null) {
                return;
            }
            //词条集合
            List<string> data = new List<string>();

            Dictionary<object, object> map = (Dictionary<object, object>)value;
            foreach (var item in map) {
                if (
                    //item.Key.ToString().Contains(this.msg)||

                    this.msg.Contains(item.Key.ToString())) {

                    JArray jas = (JArray)item.Value;

                    for (int i = 0; i < jas.Count; i++) {
                        data.Add(jas[i].ToString());
                        //this.sendMessage(jas[i].ToString());
                    }
                }
            }

            //全局回复获取
            value = Conf.getConfig("reply.group.global", null);
            if (value != null) {

                map = (Dictionary<object, object>)value;
                foreach (var item in map) {
                    if (item.Key.ToString().Contains(this.msg) || this.msg.Contains(item.Key.ToString())) {

                        JArray jas = (JArray)item.Value;

                        for (int i = 0; i < jas.Count; i++) {
                            data.Add(jas[i].ToString());
                        }
                    }
                }


            }

            /**
             * 盗图
             * 复读
             * 回复
             */
            //所有群默认为1%的几率
            double fudu = 1;
            double daotu = 1;
            double reply = 75;
            object replyCheck = Conf.getConfig("global.group." + fromGroup.ToString(), "reply");
            if (replyCheck != null) {
                try {
                    reply = double.Parse(replyCheck.ToString());
                } catch (Exception e) { }
            }

            //优化体验  防止刷屏 概率性回复 75%
            if (Util.xsd_bfb(reply)) {
                try {
                    if (data.Count != 0) {
                        //取随机一条回复
                        Random r = new Random();

                        var rand = r.Next(0, data.Count);
                        this.sendMessage(data[rand],"关键字回复");
                        //自动回复处理完成
                        return;
                    }
                } catch (Exception e) {
                    this.sendRootMessage("回复发生异常:群:" + this.fromGroup + ",QQ号:" + this.fromQQ + ",回复内容:" + this.msg + ",错误信息:" + e.Message);
                    return;
                }
            }


            //扩展事件




            //通过配置文件决定该群是否有下面的操作
            object fuduCheck = Conf.getConfig("global.group." + fromGroup.ToString(), "fudu");
            if (fuduCheck != null) {
                try {
                    fudu = double.Parse(fuduCheck.ToString());
                } catch (Exception e) {}
            }

            //盗图
            object daotuCheck = Conf.getConfig("global.group." + fromGroup.ToString(), "daotu");
            if (daotuCheck != null) {
                try {
                    daotu = double.Parse(daotuCheck.ToString());
                } catch (Exception e) {}
            }


            //概率盗图 8%
            if (Util.xsd_bfb(daotu)) {

                //this.sendMessage("触发盗图事件"+this.msg);
                string text = this.msg;
                var reg = @"^\[CQ:image,file=([A-Z0-9]+).(png|jpg|bmp|jpeg|gif)\]$";
                MatchCollection mc = Regex.Matches(text, reg);
                foreach (Match m in mc) {
                    this.sendMessage(m.Value,"概率盗图");
                    return;
                }

            }
            //概率复读 3%
            if (Util.xsd_bfb(fudu)) {
                this.sendMessage(this.msg,"概率复读");
            }

            //at自己,概率回复
            //if (Util.bfb(5) && this.msg.Contains(CQ.at(api.GetLoginQq()))) {
            //    string replyText = "";
            //    Random r = new Random();
            //    int rr = r.Next(1, 4);
            //    for (int i = 0; i <= rr; i++) {
            //        replyText += "喵";

            //        int rrrr = r.Next(0, 100);
            //        if (rrrr >= 50) {
            //            int rrr = r.Next(0, 4);
            //            for (int k = 0; k <= rr; k++) {
            //                replyText += ".";
            //            }
            //        }
            //    }
            //    rr = r.Next(1, 5);
            //    for (int i = 0; i <= rr; i++) {
            //        replyText += "?";
            //    }
            //    this.sendMessage(replyText, "atself");
            //    return;

            //}




        }


        /// <summary>
        /// 删除回复
        /// </summary>
        /// <param name="isGlobal"></param>
        private void groupReplayDel(bool isGlobal) {

            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("groupReplayDel") ==false) {
                    this.sendMessage(this.atself() + "目前不允许非管理员删除词条!");
                }
                return;
            }

            if (isGlobal && this.initCheckAdmin() == false) {
                this.sendMessage(this.atself() + "你没有权限使用删除全局词条!");
                return;
            }

            string val = this.getValue();
            if (val == null) return;
            string key = val;


            string[] txt = val.Split('-');//判断是否针对指定关键字to指定回复删除
            if (txt.Length == 2) {
                key = txt[0];
                val = txt[1];
            }





            if (this.checkEmpty(key)) {
                this.sendMessage(CQ.at(this.fromQQ) + "删除词条不能为空!");
                return;
            }
            object from = this.fromGroup;
            if (isGlobal) {
                from = "global";
            }
            //获取已有配置
            object vvv = Conf.getConfig("reply.group." + from, null);
            if (vvv == null) {
                return;
            }
            //查询是否已经存在
            List<string> list = new List<string>();
            bool isKeyHave = false;
            bool isValHave = false;
            Dictionary<object, object> map = (Dictionary<object, object>)vvv;
            foreach (var item in map) {
                if (item.Key.ToString().Equals(key)) {


                    JArray jas = (JArray)item.Value;

                    if (jas.Count != 0) {
                        isKeyHave = true;
                        if (txt.Length == 2) {
                            for (int i = 0; i < jas.Count; i++) {

                                if (jas[i].ToString() != val) {

                                    list.Add(jas[i].ToString());
                                } else {
                                    //这里是删除的字段
                                    isValHave = true;
                                }
                            }
                        } else {
                            //没有2个长度,则默认清空词条
                            isValHave = true;
                        }
                    }
                }
            }

            //检查是否存在
            if (isKeyHave == false) {
                this.sendMessage(CQ.at(this.fromQQ) + "没有找到这个词条!");
                return;
            }
            if (isValHave == false) {
                this.sendMessage(CQ.at(this.fromQQ) + "没有找到这个词条的指定回复!");
                return;
            }


            Conf.setConfig("reply.group." + from, key, list);

            this.sendMessage("删除词条成功");
        }
        /// <summary>
        /// 添加回复
        /// </summary>
        /// <param name="isGlobal"></param>
        private void groupReplayAdd(bool isGlobal) {

            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("groupReplayAdd") ==false) {
                    this.sendMessage(this.atself() + "目前不允许非管理员添加词条!");
                }
                return;
            }


            if (isGlobal && this.initCheckAdmin() == false) {
                this.sendMessage(this.atself() + "你没有权限使用添加全局词条!");
                return;
            }



            string val = this.getValue();
            if (val == null) return;
            string[] txt = val.Split('-');
            if (2 > txt.Length) {
                this.sendMessage(CQ.at(this.fromQQ) + "词条需要回复条件和回复内容!命令请查看帮助");
                return;
            }
            string key = txt[0];
            string value = val.Substring(key.Length + 1);
            if (this.clearEmpty(key) == ":") {
                this.sendMessage("我可去.....");
                return;
            }

            if (this.checkEmpty(key) || this.checkEmpty(value)) {
                this.sendMessage(CQ.at(this.fromQQ) + "词条需要回复条件和回复内容并且不能为空!命令请查看帮助");
                return;
            }
            key = this.clearEmpty(key);

            object from = this.fromGroup;
            if (isGlobal) {
                from = "global";
            }

            //获取已有配置
            object vvv = Conf.getConfig("reply.group." + from, null);
            if (vvv == null) {
                return;
            }
            //查询是否已经存在
            List<string> list = new List<string>();
            Dictionary<object, object> map = (Dictionary<object, object>)vvv;
            foreach (var item in map) {
                if (item.Key.ToString().Equals(key)) {

                    JArray jas = (JArray)item.Value;

                    for (int i = 0; i < jas.Count; i++) {
                        list.Add(jas[i].ToString());
                    }

                }
            }

            //检查是否存在
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == value) {
                    list.Remove(value);
                }
            }
            ////添加新的
            list.Add(value);




            Conf.setConfig("reply.group." + from, key, list);

            this.sendMessage("回复设置\r\n" + key + "\r\n" + value,false);

        }


        /// <summary>
        /// 添加屏蔽的QQ号
        /// </summary>
        private void addnoreply() {
            var value = this.getValue();

            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("addnoreply") ==false) {
                    this.sendMessage(this.atself() + "你没有权限使用!");
                }
                return;
            }
            long data = 0;
            try {
                data = long.Parse(value);
            } catch (Exception e) {
                this.sendMessage(this.atself() + "请输入正确的QQ号码!");
                return;
            }

            bool isNoAllow = false;
            for (int i = 0; i < this.NoReplayQQ.Count; i++) {
                if (this.NoReplayQQ[i] == data) {
                    isNoAllow = true;
                    break;
                }
            }
            if (isNoAllow) {
                this.sendMessage(this.atself() + "这个QQ号码已经设置过了!");
                return;
            }
            this.NoReplayQQ.Add(data);
            Conf.setConfig("global.config", "noreplyqq", this.NoReplayQQ);
            this.sendMessage(this.atself() + "屏蔽【" + data + "】成功!");
        }


        /// <summary>
        /// 添加允许请求的群
        /// </summary>
        private void addGroup() {
            var value = this.getValue();

            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("addGroup") ==false) {
                    this.sendMessage(this.atself() + "你没有权限使用!");
                }
                return;
            }
            long data = 0;
            try {
                data = long.Parse(value);
            } catch (Exception e) {
                this.sendMessage(this.atself() + "请输入正确的QQ群号码!");
                return;
            }

            bool isNoAllow = false;
            for (int i = 0; i < this.allSendGroup.Count; i++) {
                if (this.allSendGroup[i] == data) {
                    isNoAllow = true;
                    break;
                }
            }
            if (isNoAllow) {
                this.sendMessage(this.atself() + "这个QQ群已经设置过了!");
                return;
            }
            this.allSendGroup.Add(data);
            Conf.setConfig("global.config", "group", this.allSendGroup);
            this.sendMessage(this.atself() + "添加群【" + data + "】成功!");
        }

        /// <summary>
        /// 添加管理员
        /// </summary>
        private void addAdmin() {
            var value = this.getValue();

            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("addAdmin")==false) {
                    this.sendMessage(this.atself() + "你没有权限使用!");
           
                }
                return;
            }
            long data = 0;
            try {
                data = long.Parse(value);
            } catch (Exception e) {
                this.sendMessage(this.atself() + "请输入正确的QQ号码!");
                return;
            }

            bool isNoAllow = false;
            for (int i = 0; i < this.rootQQ.Count; i++) {
                if (this.rootQQ[i] == data) {
                    isNoAllow = true;
                    break;
                }
            }
            if (isNoAllow) {
                this.sendMessage(this.atself() + "这个管理员已经设置过了!");
                return;
            }
            this.rootQQ.Add(data);
            Conf.setConfig("global.config", "admin", this.rootQQ);
            this.sendMessage(this.atself() + "添加狗管理【" + data + "】成功!");
        }
    }
}
