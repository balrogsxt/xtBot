using com.acgxt.bot.MahuaApis.PrivateEvent;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 来自好友的私聊消息接收事件
    /// </summary>
    public class FirendPrivateMessage : IPrivateMessageFromFriendReceivedMahuaEvent {
        private readonly IMahuaApi api;

        public FirendPrivateMessage(IMahuaApi mahuaApi) {
           this.api = mahuaApi;
        }

        public void ProcessFriendMessage(PrivateMessageFromFriendReceivedContext context) {
            new PrivateMessage(context.FromQq,context.Message,PrivateMessage.TYPE.FRIEND);

        }
    }
}
