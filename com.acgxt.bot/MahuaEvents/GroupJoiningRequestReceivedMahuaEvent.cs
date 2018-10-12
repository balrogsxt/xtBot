using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 入群申请接收事件
    /// </summary>
    public class GroupJoiningRequestReceivedMahuaEvent : IGroupJoiningRequestReceivedMahuaEvent {
        private readonly IMahuaApi api;

        public GroupJoiningRequestReceivedMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessJoinGroupRequest(GroupJoiningRequestReceivedContext context) {
            api.AcceptGroupJoiningInvitation(context.GroupJoiningRequestId,context.ToGroup,context.FromQq);
        }
    }
}
