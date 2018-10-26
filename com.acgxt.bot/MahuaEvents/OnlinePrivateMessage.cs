using com.acgxt.bot.MahuaApis.PrivateEvent;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 来自在线状态的私聊消息接收事件
    /// </summary>
    public class OnlinePrivateMessage : IPrivateMessageFromOnlineReceivedMahuaEvent {
        private readonly IMahuaApi api;

        public OnlinePrivateMessage(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessOnlineMessage(PrivateMessageFromOnlineReceivedContext context) {
            new PrivateMessage(context.FromQq,context.Message,PrivateMessage.TYPE.ONLINE);
        }
    }
}
