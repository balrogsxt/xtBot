using com.acgxt.bot.Utils;
using com.acgxt.bot.Utils.XtException;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent.Core{
    class GroupEventApi {
        protected IMahuaApi api;
        protected string value;
        protected long fromGroup;
        protected string command;
        protected string[] args;
        protected List<long> rootQQ;
        protected long fromQQ;
        protected long debugQQ;
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        /// <param name="fromGroup"></param>
        /// <param name="fromQQ"></param>
        /// <param name="rootQQ"></param>
        public void __init(string command,string value,long fromGroup,long fromQQ,List<long> rootQQ,long debugQQ) {
            this.api = MahuaRobotManager.Instance.CreateSession().MahuaApi;
            this.value = value;
            this.fromGroup = fromGroup;
            this.command = command;
            this.fromQQ = fromQQ;
            this.rootQQ = rootQQ;
            this.debugQQ = debugQQ;
            this.args = value.Split(' ');
            CQAPI.xtAddLog(LogType.status.DEBUG,"RUN_MODULE","执行命令:"+command);
        }
        /// <summary>
        /// 在当前群发送群消息
        /// </summary>
        /// <param name="message"></param>
        protected void sendMessage(object message) {
            if (Util.checkEmpty(message.ToString())) {
                return;
            }
            this.api.SendGroupMessage(this.fromGroup.ToString(),CQ.CQString(message.ToString()));
        }
        public static Dictionary<string, long> yc = new Dictionary<string, long>();
        /// <summary>
        /// 延迟间隔请求过滤
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool checkSleep(int s) {
            string str = String.Format("{0}-{1}-{2}", this.fromGroup, this.fromQQ,this.command);
            if (yc.ContainsKey(str)) {
                long ls = yc[str];
                if (ls > Util.getTime()) {
                    string ss =  "命令:" + this.command + "->" + this.fromGroup + "->" + this.fromQQ + "延迟请求,解除剩余时间:"+(ls-Util.getTime())+"秒";
                    CQAPI.xtAddLog(LogType.status.DEBUG,"频繁忽略",ss);
                    //throw new NoException(ss);
                    //throw new Exception(ss);

                    return true;
                }
                yc.Remove(str);
            }
            yc.Add(str, Util.getTime() + s);
            return false;
        }
        /// <summary>
        /// 在当前群发送群消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isReplace"></param>
        protected void sendMessage(object data, bool isReplace) {
            if (Util.checkEmpty(data.ToString())) {
                return;
            }
            string message = data.ToString();
            if (isReplace) {
                message = CQ.CQString(message);
            }
            CQAPI.xtAddLog(LogType.status.DEBUG, "群聊消息", "[来自" + this.command + "发送的数据]");
            this.api.SendGroupMessage(this.fromGroup.ToString(), message);
        }
        /// <summary>
        /// 获取记录值
        /// </summary>
        /// <returns></returns>
        protected string getValue() {
            return this.value;
        }
        /// <summary>
        /// At当前发送群消息的用户
        /// </summary>
        /// <returns></returns>
        protected string atself() {
            return CQ.at(this.fromQQ);
        }
        /// <summary>
        /// 检查是否是属于管理员QQ
        /// </summary>
        /// <returns></returns>
        protected bool initCheckAdmin() {
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
        /// <summary>
        /// 非管理员回复屏蔽
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool ignoreAdmin(string key) {
            //10分钟内限定回复3次
            int max = 2;//3次重置,更新时间戳
            int num = 0;
            long liveTime = Util.getTime();
            long time = Util.getTime();
            object numObj = Conf.getConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ);
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
                CQAPI.xtAddLog(ac, LogType.status.INFO, "非管理员忽略", "存在多次使用非管理员命令【" + key + "】目前还在忽略时间内");
                return true;
            }
            //自增1
            num += 1;


            //get success
            if (num >= max) {
                //重置次数
                //增加10分钟的屏蔽

                long closeTime = liveTime += 60 * 10;

                string data = "0-" + closeTime;

                Conf.setConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ, data);
                //不执行后面操作
                CQAPI.xtAddLog(ac, LogType.status.INFO, "管理员忽略", "存在多次使用非管理员命令【" + key + "】当前开始进行10分钟忽略");
                return true;
            } else {
                string data = num + "-" + time;
                Conf.setConfig("global.group." + fromGroup + ".ignore", key + "-" + fromQQ, data);
                CQAPI.xtAddLog(ac, LogType.status.INFO, "管理员忽略", "使用非管理员命令【" + key + "】当前第" + num + "次," + max + "次后将进行忽略10分钟!");

            }
            return false;

        }
    }
}
