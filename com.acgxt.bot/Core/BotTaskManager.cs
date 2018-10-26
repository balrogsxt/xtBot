using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.acgxt.bot.Core {
    class BotTaskManager {
        private IMahuaApi api;

        private int y;
        private int m;
        private int d;
        private int h;
        private int i;
        private int s;
        private Dictionary<int,string> taskCommand = new Dictionary<int, string>();

        public BotTaskManager(string scriptFile) {
            this.api = MahuaRobotManager.Instance.CreateSession().MahuaApi;

            StreamReader sr = new StreamReader(scriptFile, Encoding.UTF8);
            String line;
            bool isSuccess = true;
            //Log.addLog("开始检测脚本信息!");
            while ((line = sr.ReadLine()) != null) {
                bool flag = this.parse(line);

                if (!flag) {
                    isSuccess = false;
                    break;
                }

            }
            sr.Close();
            if (isSuccess) {
                //Log.addLog("开始处理定时计划");
                BotTask btask = null ;
                try {
                    switch (this.type) {
                        case 1:
                            Log.addLog("BOT_TASK", String.Format("开始执行任务【{0}】,模式为每天定时运行,时间设定为{1}:{2}:{3}执行", this.name, this.h, this.i, this.s));
                            btask = new BotTask(this.h, this.i, this.s);
                            break;
                        case 2:
                            Log.addLog("BOT_TASK", String.Format("开始执行任务【{0}】,模式为间隔时间运行,应用启动后{1}秒执行一次", this.name,this.s));
                            btask = new BotTask(this.s);
                            break;
                        case 3:
                            Log.addLog("BOT_TASK", String.Format("开始执行任务【{0}】,模式为每月定时运行,时间设定为每月{1}日{2}:{3}:{4}执行", this.name,this.d, this.h, this.i, this.s));
                            btask = new BotTask(this.d, this.h, this.i, this.s);
                            break;
                        default:
                            throw new Exception("定时模式异常,当前不支持:" + this.type);
                    }
                    if (btask != null) {
                        //Log.addLog("开始监听BotTask任务");
                        btask.onTrigger += (api) => {
                            Log.addLog("Trigger BotTask->"+this.name);
                            foreach (var item in this.taskCommand) {
                                Log.addLog("Run_Command:" + item.Value);
                                this.parseCommand(item.Value);
                            }
                        };
                    } else {
                        Log.addLog("没有创建BotTask任务");
                    }
                }catch(Exception e) {
                    Log.addLog("BOT_TASK_ERR",String.Format("运行任务计划[{0}]时发生异常:{1}", this.name, e.Message));
                }
                


                

            } else {
                Log.addLog("运行任务计划异常!");
            }
        }
        private int type = 0;//执行类型
        private string name = "未定义";//计划任务名称

        private JObject checkJsonField(string[] fields,string json,string commandName) {
            if (!Util.isJson(json)) {
                Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:参数Not Json!", commandName));
                return null;
            }
            JObject data = JObject.Parse(json);
            for (int i = 0; i < fields.Length; i++) {
                if (data.Property(fields[i]) == null) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:缺少" + fields[i] + "参数!", commandName));
                    return null;
                }
            }
            return data;
        }
        private bool checkCommand(string str,string command) {
            str = str.ToLower();
            command = command.ToLower();

            if (str.Substring(0, command.Length) == command) {
                return true;
            } else {
                return false;
            }
        }
        private void parseCommand(string str) {
            string[] args = { };
            string tmp = String.Empty;
            JObject data = null;
            string
                //发送群消息 JSON
                fn_sendGroupMessage = "sendGroupMessage=",
                //发送私聊消息 JSON
                fn_sendPrivateMessage = "sendPrivateMessage=",
                //延迟时间请求 INT
                fn_sleep = "sleep=",
                //设置群公告 JSON
                fn_setNotice = "setNotice=";





            if (this.checkCommand(str, fn_sendGroupMessage)) {//发送群聊消息
                tmp = str.Substring(fn_sendGroupMessage.Length);
                string commandName = "SendGroupMessage";

                string[] fields = { "group", "message" };
                data = this.checkJsonField(fields, tmp, commandName);
                if (data==null) {
                    return;
                }

                string group = data["group"].ToString();
                string msg = data["message"].ToString();


                if (Util.checkEmpty(group)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:发送群号码不能为空!",commandName));
                    return;
                }
                if (Util.checkEmpty(msg)) {
                    Log.addLog("XT_SCRIPT",String.Format("执行{0}异常:消息内容不能为空!",commandName));
                    return;
                }
                Log.addLog("XT_SCRIPT", String.Format("{0}:{1}->{2}",commandName,group,msg));
                //run
                this.api.SendGroupMessage(group, msg);
            }else if (this.checkCommand(str, fn_sendPrivateMessage)) {//发送私聊消息
                tmp = str.Substring(fn_sendPrivateMessage.Length);
                string commandName = "SendPrivateMessage";


                string[] fields = { "qq", "message" };
                data = this.checkJsonField(fields, tmp, commandName);
                if (data == null) {
                    return;
                }
                string qq = data["qq"].ToString();
                string msg = data["message"].ToString();
                if (Util.checkEmpty(qq)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:发送QQ号码不能为空!", commandName));
                    return;
                }
                if (Util.checkEmpty(msg)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:消息内容不能为空!", commandName));
                    return;
                }
                Log.addLog("XT_SCRIPT",String.Format("{0}:{1}->{2}", commandName, qq, msg));
                //run
                this.api.SendPrivateMessage(args[0], args[1]);
            } else if (this.checkCommand(str, fn_sleep)) {//延迟执行
                string commandName = "Sleep";
                tmp = str.Substring(fn_sleep.Length);

                try {
                    int s = int.Parse(tmp);
                    Log.addLog("XT_SCRIPT", String.Format("{0}:{1}秒->start", commandName, s));
                    Thread.Sleep(s * 1000);
                    Log.addLog("XT_SCRIPT", String.Format("{0}:{1}秒->end", commandName, s));
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:延迟秒数必须为数字!", commandName));
                    return;
                }
            } else if (this.checkCommand(str, fn_setNotice)) {//延迟执行
                string commandName = "setNotice";
                tmp = str.Substring(fn_setNotice.Length);

                string[] fields = { "group", "title", "content" };
                data = this.checkJsonField(fields, tmp, commandName);
                if (data == null) {
                    return;
                }

                string group = data["group"].ToString();
                string title = data["title"].ToString();
                string content = data["content"].ToString();


                if (Util.checkEmpty(group)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:发送群号码不能为空!", commandName));
                    return;
                }
                if (Util.checkEmpty(title)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:公告标题不能为空!", commandName));
                    return;
                }
                if (Util.checkEmpty(content)) {
                    Log.addLog("XT_SCRIPT", String.Format("执行{0}异常:公告内容不能为空!", commandName));
                    return;
                }
                Log.addLog("XT_SCRIPT", String.Format("{0}:{1}->{2}=>{3}", commandName, group, title,content));
                //run
                this.api.SetNotice(group,title,content);
            } else {
                Log.addLog("XT_SCRIPT", String.Format("不支持的脚本命令:{0}",str));
                return;
            }
            
        }
        private bool parse(string str) {

            //配置文件优先检查
            string typeKey = "[type]";
            string nameKey = "[name]";
            string yKey = "[y]";
            string mKey = "[m]";
            string dKey = "[d]";
            string hKey = "[h]";
            string iKey = "[i]";
            string sKey = "[s]";
            if (str.Substring(0, yKey.Length) == yKey) {
                try {
                    this.y = int.Parse(str.Substring(yKey.Length));
                    //Log.addLog("【参数Y】" + this.y);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数Y异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
            if (this.checkCommand(str, mKey)) {
                try {
                    this.m = int.Parse(str.Substring(mKey.Length));
                    //Log.addLog("【参数M】" + this.m);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数M异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
            if (this.checkCommand(str, dKey)) {
                try {
                    this.d = int.Parse(str.Substring(dKey.Length));
                    //Log.addLog("【参数D】" + this.d);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数D异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }

            if (this.checkCommand(str, hKey)) {
                try {
                    this.h = int.Parse(str.Substring(hKey.Length));
                    //Log.addLog("【参数H】" + this.h);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数H异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
            if (this.checkCommand(str, iKey)) {
                try {
                    this.i = int.Parse(str.Substring(iKey.Length));
                    //Log.addLog("【参数I】" + this.i);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数I异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
            if (this.checkCommand(str, sKey)) {
                try {
                    this.s = int.Parse(str.Substring(sKey.Length));
                    //Log.addLog("【参数S】" + this.s);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "任务参数S异常错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
















            if (this.checkCommand(str, typeKey)) {//get type
                try {
                    this.type = int.Parse(str.Substring(typeKey.Length));
                    Log.addLog("BotTaskType:" + this.type);
                } catch (Exception e) {
                    Log.addLog("XT_SCRIPT", "获取任务计划类型错误!该任务已被终止运行!");
                    return false;
                }
                return true;
            }
            if (this.checkCommand(str, nameKey)) {
                this.name = str.Substring(nameKey.Length);
                Log.addLog("BotTaskName:" + this.name);
                return true;
            }
            if (!Util.checkEmpty(str)) {
                this.taskCommand.Add(this.taskCommand.Count, str);
            }
            return true;


        }




    }
}
