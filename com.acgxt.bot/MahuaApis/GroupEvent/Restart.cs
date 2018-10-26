using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.MahuaApis.Module;
using com.acgxt.bot.Utils;
using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class Restart : GroupEventApi, GroupEvents {
        public void run() {
            if (this.initCheckAdmin() == false) {
                if (this.ignoreAdmin("cqp_restart") == false) {
                    this.sendMessage(this.atself() + "您没有权限操作机器人重启功能!");
                }
                return;
            }

            CQRestart cqp = new CQRestart();
            cqp.onLogger += (s) => {
                MahuaRobotManager.Instance.CreateSession().MahuaApi.SendGroupMessage(fromGroup.ToString(), CQ.at(fromQQ) + s);
            };
            cqp.run(this.fromGroup.ToString(), this.fromQQ.ToString());

        }
    }
}
