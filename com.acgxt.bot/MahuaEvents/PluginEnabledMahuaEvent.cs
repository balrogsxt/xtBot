using com.acgxt.bot.Core;
using com.acgxt.bot.MahuaApis.Module;
using com.acgxt.bot.Utils;
using com.acgxt.bot.Utils.Server;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 插件被启用事件
    /// </summary>
    public class PluginEnabledMahuaEvent: IPluginEnabledMahuaEvent {
        private readonly IMahuaApi api;

        public PluginEnabledMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void Enabled(PluginEnabledContext context) {


            //基础配置
            //机器人任务计划
            Log.addLog("INIT", String.Format("插件启用,当前登录账号:{0}({1})", api.GetLoginNick(), api.GetLoginQq()));
            //初始化百度AI
            Conf.baiduAiInit();
            //酷Q模拟重启初始化获取句柄
            Log.info("开始尝试获取酷Q窗口句柄");
            try {
                CQRestart.__init();
                Log.info("获取酷Q窗口句柄成功,可以执行模拟重启功能");
            } catch (Exception e) {
                Log.warning("获取酷Q窗口句柄失败:" + e.Message);
            }
            string qq = Conf.getStringConfig("global.restart", "qq", "0");
            string group = Conf.getStringConfig("global.restart", "group", "0");
            long time = Conf.getLongConfig("global.restart", "time",0);
            if (qq!="0"||group!="0") {
                string s = "";
                if (time!=0) {
                    s = ",本次重启耗时:"+(Util.getTime() - time) +"秒";
                }



                if (group=="0"&&qq!="0") {
                    //私聊
                    this.api.SendPrivateMessage(qq,CQ.shake());
                    this.api.SendPrivateMessage(qq, "酷Q已重启完成!"+s);
                } else if(group!="0"){
                    if (qq=="0") {
                        this.api.SendGroupMessage(group,"酷Q已重启完成!"+s);
                    } else {
                        this.api.SendGroupMessage(group, CQ.at(qq) + "酷Q已重启完成!"+s);
                    }
                } else {
                    Log.addLog("未知的请求人重启");
                }


                Conf.setConfig("global.restart","none");
            }




            Task.Factory.StartNew(() =>{
                using (var robotSession = MahuaRobotManager.Instance.CreateSession()) {
                    var api = robotSession.MahuaApi;
                    UdpServer server = new UdpServer();
                    server.run(api);
                }
            });
            Task.Factory.StartNew(() => {

                



                string taskDir = Path.Combine(Directory.GetCurrentDirectory(), "bottask");
                if (!Directory.Exists(taskDir)) {
                    Directory.CreateDirectory(taskDir);
                }
                DirectoryInfo root = new DirectoryInfo(taskDir);
                foreach (FileInfo file in root.GetFiles()) {
                    if (file.Name.Substring(file.Name.Length - 3) == ".xt") {
                        Log.addLog("DEBUG", "---------------------"+ file.Name + "------------------------------------");
                        Log.addLog("DEBUG", "载入【" + file.Name+"】脚本文件进行处理");
                        new BotTaskManager(file.FullName);
                    }
                }
            });

            

        }
    }
}
