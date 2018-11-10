using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis {
    public class QQ {
        private IMahuaApi api;
        private long group;
        private long qq;
        public static QQ getQQ(object qq) {
            return new QQ(qq);
        }


        private QQ(object qq) {
            this.api = MahuaRobotManager.Instance.CreateSession().MahuaApi;
            this.qq = long.Parse(qq.ToString());

        }

        public void sendGroupMessage(object message) {
            this.api.SendGroupMessage(this.group.ToString(), message.ToString());
        }
        public void sendGroupMessage(object group,object message) {
            this.api.SendGroupMessage(group.ToString(), message.ToString());
        }
        public void sendPrivateMessage(object privateQQ,object message) {
            this.api.SendPrivateMessage(privateQQ.ToString(),message.ToString());
        }

    }
}
