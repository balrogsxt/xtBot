using com.acgxt.bot.MahuaApis.Module;
using com.acgxt.cqp.cs.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    class CQ {
        private static string liveDir = Directory.GetCurrentDirectory();


        public static string audio(string fileName) {
            string dirPath = Path.Combine(liveDir, "data/record/xtaudio");
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            string rePath = @"xtaudio/" + fileName;

            return String.Format("[CQ:record,file={0}]",rePath);


        }
        /// <summary>
        /// 字符串替换处理
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string CQString(string msg) {

            //检查是否有全局词条格式
            try {
                var reg = @"\<(.*?)\>";
                MatchCollection mc = Regex.Matches(msg, reg);
                foreach (Match m in mc) {
                    string value = Conf.getVar(m.Groups[1].ToString(), m.Groups[0].ToString());

                    msg = msg.Replace(m.Groups[0].ToString(), value);
                }
            } catch (Exception e) {
                Log.warning("转换全局词条发生异常:" + e.Message);
            }

            //转换为特殊格式
            msg = msg.Replace("{{", "[");
            msg = msg.Replace("}}", "]");


            




            return msg;
        }

        public static string httpImage(string httpUrl,string ext) {

            try {
                WebClient wc = new WebClient();

                string dirPath = Path.Combine(liveDir, "data/image/xtimages");
                if (!Directory.Exists(dirPath)) {
                    Directory.CreateDirectory(dirPath);
                }
                string fileName = Util.md5(httpUrl) + "." + ext;
                string filePath = Path.Combine(dirPath,fileName) ;
                //获取相对路径
                string rePath = @"xtimages/" + fileName;

                //判断是否已经下载图片
                if (File.Exists(filePath)) {
                    return String.Format("[CQ:image,file={0}]", rePath);
                }
                wc.DownloadFile(httpUrl, filePath);
                return String.Format("[CQ:image,file={0}]", rePath);
            }catch(Exception e) {
                Log.error("下载图片失败:" + e.Message);
                return "获取图片失败";
            }
        }

        public static string getRandomString(int size) {
            string[] strs = { "A", "B", "C", "D", "E", "F", "G", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U" };
            string str = Util.getTime().ToString();
            Random r = new Random();
            for (int i=0;i<size;i++) {
                str += strs[r.Next(0, strs.Length)];
            }
            return str;
        }



        //public static string udpsendExe = Path.Combine(liveDir, @"udpsend.exe");
        //public static int ac = CQAPI.getAuthCode();
        /// <summary>
        /// at码
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        public static string at(object qq) {
            return String.Format("[CQ:at,qq={0}]",qq);
        }
        /// <summary>
        /// 窗口抖动(shake) - 仅支持好友
        /// </summary>
        /// <returns></returns>
        public static string shake() {
            return "[CQ:shake]";
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static string image(string imagePath) {
            return String.Format("[CQ:image,file={0}]", imagePath);
        }
        /// <summary>
        /// 发送emoji表情
        /// </summary>
        /// <param name="emojiId"></param>
        /// <returns></returns>
        public static string emoji(string emojiId) {
            return String.Format("[CQ:emoji,id={0}]", emojiId);

        }
        /// <summary>
        /// share分享
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string share(string url,string title,string content,string image) {
            string code = String.Format(",url={0}", url);
            if (!Util.checkEmpty(title)) {
                code += String.Format(",title={0}", title);
            }
            if (!Util.checkEmpty(content)) {
                code += String.Format(",content={0}", content);
            }
            if (!Util.checkEmpty(image)) {
                code += String.Format(",image={0}", image);
            }
            return String.Format("[CQ:share{0}]", code);
        }


        //public static UdpClient udpClient = null;
        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="msgid">消息ID</param>
        /// <returns></returns>
        public static int deleteMessage(long msgid){
            int ac = CQAPI.getAuthCode();
            if (msgid==(-10)) {
                CQAPI.xtAddLog(ac, LogType.status.INFO, "撤回消息", "无法撤回消息:查询msgid发生异常,状态码【-10】");
                return 1;
            } else if(msgid==0){
                CQAPI.xtAddLog(ac, LogType.status.INFO, "撤回消息", "无法撤回消息:没有找到所属数据,状态码【0】");
                return 1;
            }else if (msgid == (-1)) {
                CQAPI.xtAddLog(ac, LogType.status.INFO, "撤回消息", "无法撤回消息:没有获取到当前登陆QQ号码,状态码【-1】");
                return 1;
            }
            return CQAPI.xtDeleteMessage(ac, msgid);
        }
        /// <summary>
        /// 获取图片的外链地址
        /// </summary>
        /// <param name="imageFileName"></param>
        /// <returns></returns>
        public static string getImageUrl(string imageFileName) {
            var reg = @"^\[CQ:image,file=([A-Z0-9]+).(png|jpg|bmp|jpeg|gif)\]$";
            MatchCollection mc = Regex.Matches(imageFileName, reg);
            bool fileExists = false;
            string filePath = "";
            foreach (Match m in mc) {
                string fileName = m.Groups[1] + "." + m.Groups[2]+ ".cqimg";

                filePath = Path.Combine(liveDir, "data/image/" + fileName);
                if (File.Exists(filePath)) {
                    fileExists = true;
                }

            }
            if (!fileExists) {
                return "null";
            }

            string data = getIni("image", "url",filePath);
            return data;
        }
        public static long getShareGroup(string cqCode) {
            var reg = @"^\[CQ:contact,id=([0-9]+),type=group\]$";
            MatchCollection mc = Regex.Matches(cqCode, reg);
            bool fileExists = false;
            string filePath = "";
            foreach (Match m in mc) {
                return long.Parse(m.Groups[1].ToString());
            }
            return 0;

        }
        


        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,string val, string filePath);

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key,string def, StringBuilder retVal, int size, string filePath);
        public static string getIni(string section, string key, string path) {
            StringBuilder stringBuilder = new StringBuilder(1024);
            GetPrivateProfileString(section, key, "", stringBuilder, 1024, path);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取群员列表数据
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static List<GroupUser> getGroupUserList(long group) {
            int ac = CQAPI.getAuthCode();
            string jsonData = CQAPI.getGroupUserList(ac, group);
            try {
                JArray list = JArray.Parse(jsonData);
                List<GroupUser> data = new List<GroupUser>();
                for (int i = 0; i < list.Count; i++) {
                    GroupUser gu = new GroupUser();
                    gu.setName(list[i]["name"].ToString());
                    gu.setQQId(int.Parse(list[i]["qqid"].ToString()));
                    gu.setCard(list[i]["card"].ToString());
                    gu.setSex(int.Parse(list[i]["sex"].ToString()));
                    gu.setAge(int.Parse(list[i]["age"].ToString()));
                    gu.setAddress(list[i]["address"].ToString());

                    gu.setJoinTime(long.Parse(list[i]["joinTime"].ToString()));
                    gu.setLastTime(long.Parse(list[i]["lastTime"].ToString()));
                    gu.setRule(int.Parse(list[i]["rule"].ToString()));
                    data.Add(gu);
                }
                return data;

            } catch (Exception e) {
                return null;
            }
        }
        /// <summary>
        /// 获取群员列表数量
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static int getGroupUserListSize(long group) {
            int ac = CQAPI.getAuthCode();
            string jsonData = CQAPI.getGroupUserList(ac, group);
            try {
                JArray list = JArray.Parse(jsonData);
                return list.Count;
            } catch (Exception e) {
                return 0;
            }
        }

        /// <summary>
        /// 获取群成员列表数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        public static string getGroupUserList(string group,string cookie,string bkn) {
            try {
                //data
                //string cookieStr = "pgv_pvi=3977810944; pgv_pvid=3721661967; eas_sid=N1b5A3A4i42109F4A1O257a7E6; RK=wGZsnRgwwq; ptcz=fa5bc7aed4d0d1f9b2f20baa287acb9bcd9fd669a4ecf7c8a5ec2d9ea33d7144; tvfe_boss_uuid=1ef5ce08e4615baa; ptui_loginuin=2049431303; o_cookie=2049431303; pt2gguin=o2289453456; _qpsvr_localtk=0.2021083549883953; pgv_si=s2634632192; ptisp=ctc; uin=o2289453456; skey=@GvKU7KqIG; p_uin=o2289453456; pt4_token=2AVYTSAFPO*PuyT4miKrdlSSH03Tr0TvjWyFbBpfyB0_; p_skey=ZUDqpq2uprgAf8IngI1wjcfmj2YOd5QsJjGK86KkPbk_";
                string cookieStr = cookie;
                string postData = string.Format("bkn={0}&gc={1}&st=0&end=20&sort=0",bkn,group);
                byte[] data = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://qun.qq.com/cgi-bin/qun_mgr/search_group_members");
                request.Method = "POST";
                request.Referer = "https://qun.qq.com";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                //request.Host = "https://qun.qq.com";
                request.Headers.Add("Cookie", cookieStr);
                request.ContentLength = data.Length;
                Stream newStream = request.GetRequestStream();


                newStream.Write(data, 0, data.Length);
                newStream.Close();

                HttpWebResponse myResponse = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                return content;
            } catch (Exception e) {
                return e.Message;
            
            }

        }


    }
}
