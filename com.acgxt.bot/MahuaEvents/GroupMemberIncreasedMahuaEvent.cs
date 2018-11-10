using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace com.acgxt.bot.MahuaEvents {
    /// <summary>
    /// 群成员增多事件
    /// </summary>
    public class GroupMemberIncreasedMahuaEvent: IGroupMemberIncreasedMahuaEvent {
        private readonly IMahuaApi api;

        public GroupMemberIncreasedMahuaEvent(IMahuaApi mahuaApi) {
            this.api = mahuaApi;
        }

        public void ProcessGroupMemberIncreased(GroupMemberIncreasedContext context) {



            string liveQQ = api.GetLoginQq();
            string joinQQ = context.JoinedQq;
            string group = context.FromGroup;


            if (group=="734717901") {
                api.SendPrivateMessage("2289453456", "已加入734717901群");
                return;
            }


            int ac = CQAPI.getAuthCode();//获取authcode
            if (!liveQQ.Equals(joinQQ)) {
                //其他用户加入群
                CQAPI.xtAddLog(ac, LogType.status.INFO, "新用户加入", String.Format("加入了群:{0}",group));

                string text;
                //特殊群处理
                if (group== "454493790"||group == "854270226") {


                    //int count = CQ.getGroupUserListSize(long.Parse(group));
                    //api.SetGroupMemberCard(group, joinQQ, String.Format("卡秋莎{0}号",count));


                    //text = String.Format("欢迎{0}加入卡秋莎养殖中心,您是第{1}位卡秋莎,凡是中暑,打架,淋雨,吃太多,吃太少,不吃,受内伤的我们不如....", CQ.at(joinQQ),count);

                    text = String.Format("欢迎{0}加入本群!", CQ.at(joinQQ));

                } else {
                    text = String.Format("欢迎{0}加入本群!", CQ.at(joinQQ));

                }









                this.api.SendGroupMessage(group, text);






            } else {
                CQAPI.xtAddLog(ac, LogType.status.INFO, "自己加群", String.Format("加入了群:{0}",group));
                
                
                //将该群添加白名单
                
                
                //自己加入群
                string text = String.Format("はじめまして、どうぞよろしくお願いします、ヾ(^▽^*)))");
                this.api.SendGroupMessage(group, text);


                //载入允许群

                List<string> allSendGroup = new List<string>();

                //载入允许群
                object allowgroup = Conf.getConfig("global.config", "group");
                JArray jarr;
                try {
                    jarr = (JArray)allowgroup;
                    for (int i = 0; i < jarr.Count; i++) {
                        try {
                            string v = jarr[i].ToString();
                            if (!allSendGroup.Contains(v)) {
                                allSendGroup.Add(v);
                            }
                        } catch (Exception e) {

                        }
                    }
                } catch (Exception e) {

                }

                bool isNoAllow = false;
                for (int i = 0; i < allSendGroup.Count; i++) {
                    if (allSendGroup[i] == group) {
                        isNoAllow = true;
                        break;
                    }
                }
                if (isNoAllow) {
                    //该群已存在
                    CQAPI.xtAddLog(ac, LogType.status.INFO, "添加白名单群", String.Format("已存在白名单群:{0}", group));
                    return;
                }
                allSendGroup.Add(group);
                Conf.setConfig("global.config", "group", allSendGroup);
                CQAPI.xtAddLog(ac, LogType.status.SUCCESS, "添加白名单群", String.Format("添加白名单成功群:{0}", group));









            }



        }
    }
}
