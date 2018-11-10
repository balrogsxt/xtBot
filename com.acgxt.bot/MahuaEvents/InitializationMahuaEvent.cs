using com.acgxt.bot.Utils.Server;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 插件初始化事件
    /// </summary>
    public class InitializationMahuaEvent: IInitializationMahuaEvent {
        private readonly IMahuaApi api;

        public InitializationMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void Initialized(InitializedContext context) {
        }
    }
}
