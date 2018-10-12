using com.acgxt.bot.Utils;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 入群邀请接收事件
    /// </summary>
    public class GroupJoiningInvitationReceivedMahuaEvent
        : IGroupJoiningInvitationReceivedMahuaEvent {
        private readonly IMahuaApi api;

        public GroupJoiningInvitationReceivedMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessJoinGroupRequest(GroupJoiningRequestReceivedContext context) {
            string joinId = context.GroupJoiningRequestId;
            string group = context.ToGroup;
            string qq = context.FromQq;
            //直接允许邀请入群
            int ac = CQAPI.getAuthCode();
            CQAPI.xtAddLog(ac, LogType.status.INFO, "邀请入群",String.Format("QQ号码:{0},邀请加入群:{1}",qq,group));

            this.api.AcceptGroupJoiningInvitation(joinId, group,qq);



        }
    }
}
