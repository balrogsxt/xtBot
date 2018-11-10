using com.acgxt.bot.MahuaApis.GroupEvent.Core;
using com.acgxt.bot.Utils;
using com.acgxt.cqp.cs.Utils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.MahuaApis.GroupEvent {
    class GetFlight : GroupEventApi, GroupEvents {
        public void run() {
            if (this.checkSleep(4)) return;

            //获取航班号
            string number = this.getValue();
            if (Util.checkEmpty(number)) {
                this.sendMessage(CQ.at(this.fromQQ)+"请输入需要查询的航班号码!");
                return;
            }
            string liveDay = DateTime.Now.ToString("yyyyMMdd");
            string day = liveDay;

            if (this.args.Length==2) {
                number = this.args[0];
                day = this.args[1];
            }

            if (!Util.isRegex("^[A-Z0-9]+$",number)) {
                this.sendMessage(CQ.at(this.fromQQ) + "请输入正确的航班号码!");
                return;
            }
            if (!Util.isRegex("^[0-9]{8}$",day)) {
                this.sendMessage(CQ.at(this.fromQQ) + "请输入正确的航班日期,格式:【"+liveDay+"】!");
                return;

            }

            //今天的航班


            try {
                
                string apiUrl = String.Format("http://flights.ctrip.com/actualtime/fno--{0}-{1}.html", number, day);

                Http http = new Http(apiUrl);
                http.setEncoding("GBK");
                string html = http.getResult();

                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(html);
                HtmlNode info = doc.DocumentNode.SelectSingleNode("//div[@class='detail-info']");

                HtmlNode company = info.SelectSingleNode("//div[@class='detail-t']/span[1]");
                HtmlNode no = info.SelectSingleNode("//div[@class='detail-t']/strong[1]");
                HtmlNode flyTime = info.SelectSingleNode("//div[@class='detail-t']/span[2]");
                HtmlNode flyStartTime = info.SelectSingleNode("//div[@class='detail-fly']//div[@class='inl departure']/p[@class='time']");
                HtmlNode flyEndTime = info.SelectSingleNode("//div[@class='detail-fly']/div[@class='inl arrive']/p[@class='time']");

                HtmlNode flyStartAddress = info.SelectSingleNode("//div[@class='detail-fly detail-route']//div[@class='inl departure']/p[1]");
                HtmlNode flyEndAddress = info.SelectSingleNode("//div[@class='detail-fly detail-route']/div[@class='inl arrive']/p[1]");
                HtmlNode flyCountTime = info.SelectSingleNode("//div[@class='detail-fly detail-route']/div[@class='inl between']/p[@class='gray']");

                this.sendMessage(CQ.at(this.fromQQ) + "\r\n已查到航班"+no.InnerText.Trim()+"的信息\r\n"
                    + "航空公司:"+company.InnerText.Trim()+"\r\n"
                    + "航班时间:"+flyTime.InnerText.Trim()+"\r\n"
                    + "预计时间:"+flyStartTime.InnerText.Trim()+"—"+flyEndTime.InnerText.Trim()+"\r\n"
                    + "起飞地点:"+flyStartAddress.InnerText.Trim().Replace(" ","").Replace("\n","-")+"\r\n"
                    + "降落地点:"+flyEndAddress.InnerText.Trim().Replace(" ","").Replace("\n","-")+"\r\n"
                    +"飞行时长:"+flyCountTime.InnerText.Trim().Replace("飞行时长","").Replace("h","小时").Replace("m","分"));
            } catch (ArgumentNullException e) {
                Log.addLog("GET_FLIGHT", "查询航班异常:" + e.Message);
                this.sendMessage(CQ.at("查询航班失败,系统错误"));
            } catch (Exception e) {

                string y = day.Substring(0, 4);
                string m = day.Substring(4, 2);
                string d = day.Substring(6, 2);


                this.sendMessage(CQ.at(this.fromQQ) + "没有查找到" + String.Format("{0}年{1}月{2}日",y,m,d) + "航班号为" + number + "的航班信息\r\n" +
                    "自定义时间请发送【#查航班 "+number+" " + day + "】进行查询指定日期航班");

            }
            







        }
    }
}
