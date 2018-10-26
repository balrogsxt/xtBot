using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.PrivateEvent {
    class PrivateMessage {
        private IMahuaApi api;
        private string qq;
        private string msg;
        private TYPE type;
        public enum TYPE {
            FRIEND=1,
            GROUP=2,
            ONLINE=3

        }
        private void sendMessage(object msg) {
            if (Util.checkEmpty(msg.ToString())) {
                return;
            }
            this.api.SendPrivateMessage(this.qq, CQ.CQString(msg.ToString()));
        }
        private void sendMessage(object msg,bool isReplace) {
            if (Util.checkEmpty(msg.ToString())) {
                return;
            }
            string message = msg.ToString();
            if (isReplace) {
                message = CQ.CQString(message);
            }
            this.api.SendPrivateMessage(this.qq, message);
        }

        public PrivateMessage(string qq,string message,TYPE type) {
            this.api = MahuaRobotManager.Instance.CreateSession().MahuaApi;
            this.qq = qq;
            this.msg = message;
            this.type = type;
            if (type==TYPE.FRIEND) {
                this.sendMessage(CQ.shake());
                this.sendMessage("?");
                return;
            } else {
                this.sendMessage("暂不支持");
            }

        }

    }
}
