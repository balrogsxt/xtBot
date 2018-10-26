using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class Cha : GroupEventApi, GroupEvents {
        public void run() {
            if (this.checkSleep(5)) return;



            string[] msgList = {
                "查nm查",
                "查尼玛查 Gun!",
                "就知道查 查锤子查",
                "查锤子 滚啊",
                "天天就知道查这样查那样,成事不足败事有余",
                "套你猴子",
                "干你凉",
                "不准查",
                "俏丽吗",
                "你查锤子查",
                "说了多少遍 查不了 查不了!",
                "你自己百度啊",
                "你自己查谷歌啊",
                "你什么都想查",
                "咕咕咕",
                "我一定去查",
                "等会就去查"
            };
            Random r = new Random();

            this.sendMessage(msgList[r.Next(0, msgList.Length)]);



        }
    }
}
