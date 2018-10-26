using com.acgxt.bot.MahuaApis.PrivateEvent;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 来自群成员的私聊消息接收事件
    /// </summary>
    public class GroupPrivateMessage : IPrivateMessageFromGroupReceivedMahuaEvent {
        private readonly IMahuaApi api;

        public GroupPrivateMessage(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessGroupMessage(PrivateMessageFromGroupReceivedContext context) {
            new PrivateMessage(context.FromQq,context.Message,PrivateMessage.TYPE.GROUP);
        }
    }
}
