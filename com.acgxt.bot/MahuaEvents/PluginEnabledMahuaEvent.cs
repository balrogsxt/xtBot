using com.acgxt.bot.Utils;
using com.acgxt.bot.Utils.Server;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
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
            Log.addLog("INIT",String.Format("插件启用,当前登录账号:{0}({1})", api.GetLoginNick(), api.GetLoginQq()));
            //初始化百度AI
            Conf.baiduAiInit();
            //基础配置

            Task.Factory.StartNew(() =>{
                using (var robotSession = MahuaRobotManager.Instance.CreateSession()) {
                    var api = robotSession.MahuaApi;
                    UdpServer server = new UdpServer();
                    server.run(api);
                }
            });
        

            //new Thread(o => {

            //    UdpServer server = new UdpServer();
            //    server.run();
            //}).Start();
            
        }
    }
}
