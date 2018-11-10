using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class Help : GroupEventApi, GroupEvents {

        //每位用户只允许10秒使用一次帮助命令
        public void run() {
            if (this.checkSleep(10)) return;

            string msg = this.atself() + "帮助列表";
            string[] helps = {
                        "发送 \"#查快递\" 加订单号可查询快递物流信息",
                        "发送 \"#快递通知\" 可将上次查询的快递状态实时通知",
                        "发送 \"#二维码\" 获取输入信息的二维码图片",
                        "发送 \"#icon\" 获取输入的Url地址Favicon图片",
                        "发送 \"#查装备\" 加装备名称可查询DNF装备信息",
                        "发送 \"#查拍卖\" 加拍卖商品名称可查询最近交易价格",
                        "发送 \"#查航班\" 加航班号可查询今日航班信息",
                        "",
                        "命令使用方法例如:#查快递 [参数](多参数用空格隔开)",
                        "以上命令为可以使用的功能(不包含管理员命令)",
                        "机器人源码地址:https://git.io/fxrLl",
                        "但不代表最新版本,反馈请联系i@acgxt.com"
                    };
            for (int i = 0; i < helps.Length; i++) {
                msg += "\r\n" + helps[i];
            }
            this.sendMessage(msg);

        }
    }
}
