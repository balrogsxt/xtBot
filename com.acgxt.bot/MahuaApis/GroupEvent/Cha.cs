using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class Cha : GroupEventApi, GroupEvents {
        public void run() {
            if (this.checkSleep(5)) return;


            Random rr = new Random();
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
                "等会就去查",
                CQ.share("https://www.baidu.com/s?wd="+this.getValue(),"查到"+this.getValue()+"的信息","已在百度找到"+this.getValue()+"的信息",""),
                "已找到"+this.getValue()+"的"+rr.Next(1,10)+"条信息,查看详情请把头伸过来"
            };
            Random r = new Random();

            this.sendMessage(msgList[r.Next(0, msgList.Length)]);



        }
    }
}
