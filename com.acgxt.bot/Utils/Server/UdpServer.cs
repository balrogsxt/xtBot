using com.acgxt.cqp.cs.Utils;
using Newbe.Mahua;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils.Server {
    class UdpServer {
        private IMahuaApi api;
        public void run(IMahuaApi api) {
            this.api = api;

            object portV = Conf.getConfig("global.config", "port");
            int port = 10268;
            if (portV != null) {
                try {
                    port = int.Parse(portV.ToString());
                } catch (Exception e) {

                }
            }
            Log.addLog("UDP服务已启动,端口:"+port);

            int recv;
            byte[] bytes = new byte[1024];
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(ip);
            while (true) {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                recv = server.ReceiveFrom(bytes, ref Remote);
                string data = System.Text.Encoding.UTF8.GetString(bytes, 0, recv);


                int ac = CQAPI.getAuthCode();
                CQAPI.xtAddLog(ac, LogType.status.DEBUG, "UDP获取数据", "获取数据:" + data);
                try {
                    this.parseUdpData(data);
                } catch (Exception e) {
                    CQAPI.xtAddLog(ac, LogType.status.INFO, "UDP解析警告",e.Message);
                }
            }
  

        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        private void parseUdpData(string data) {
            if (data==""||data.Length==0||data.Trim().Length==0) {
                throw new Exception("UDP请求数据为空");
            }
            if (!Util.isJson(data)) {
                throw new Exception("UDP请求数据非JSON数据");
            }


            JObject obj = JObject.Parse(data);

            if (obj.Property("token") == null) {
                throw new Exception("验证失败:无token数据");
            }
            //获取token
            object tokenV = Conf.getConfig("global.config", "token");
            if (tokenV == null) {
                throw new Exception("验证失败:token服务端未设定,无法调用接口");
            }


            string token = obj["token"].ToString();
            if (token != tokenV.ToString()) {
                throw new Exception("验证失败:token错误,无法调用接口");
            }

            if (obj.Property("fn") == null) {
                throw new Exception("UDP数据异常");
            }


            string fnName = obj["fn"].ToString();



            switch (fnName) {
                    //发送群消息
                case "sendGroupMessage":

                    if (Util.checkEmpty(obj.Property("group"))) {
                        throw new Exception("群号码不能为空");
                    }
                    if (Util.checkEmpty(obj.Property("message"))) {
                        throw new Exception("群消息不能为空");
                    }

                    api.SendGroupMessage(obj["group"].ToString(),CQ.CQString(obj["message"].ToString()));

                    break;
                    //发送私聊消息
                case "sendPrivateMessage":
                    if (Util.checkEmpty(obj.Property("qq"))) {
                        throw new Exception("私聊QQ号码不能为空");
                    }
                    if (Util.checkEmpty(obj.Property("message"))) {
                        throw new Exception("私聊消息不能为空");
                    }

                    api.SendPrivateMessage(obj["qq"].ToString(), CQ.CQString(obj["message"].ToString()));

                    break;
                default:
                    throw new Exception(String.Format("没有找到【{0}】事件",fnName));

            }

             



        }


    }
}
