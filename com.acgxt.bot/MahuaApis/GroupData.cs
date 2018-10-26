
using com.acgxt.bot.Utils;
using com.acgxt.bot.Utils.XtException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.acgxt.cqp.cs.Utils {
    public class GroupData {


        public static int maxSize = 3;//2条触发
        public static Dictionary<long, Dictionary<int, Dictionary<string, string>>> message = new Dictionary<long, Dictionary<int, Dictionary<string, string>>>();

        public static void addMessage(long group,int msgid,long qq,string msg) {
            //临时群列表数据
            Dictionary<int, Dictionary<string, string>> tmpMsg = new Dictionary<int, Dictionary<string, string>>();
            Dictionary<int, Dictionary<string, string>> tmpMsg2 = new Dictionary<int, Dictionary<string, string>>();//最终储存

            //当前群消息
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("qq", qq.ToString());
            data.Add("msg", msg);

            if (GroupData.message.ContainsKey(group)) {//判断是否已经存在这个群的记录
                tmpMsg = GroupData.message[group]; 
            }
            tmpMsg.Add(msgid,data);
            if (tmpMsg.Count>maxSize) {//判断是否超出
                //删除第一条数据
                int i = 0;
                foreach (var item in tmpMsg) {
                    if (i!=0) {//排除第一天数据,其他全部保存
                        tmpMsg2.Add(item.Key,item.Value);
                    }
                    i++;
                }
            } else {
                tmpMsg2 = tmpMsg;
            }
            if (GroupData.message.ContainsKey(group)) {
                GroupData.message.Remove(group);
            }
            GroupData.message.Add(group,tmpMsg2);
            log("当前储存" + GroupData.message[group].Count + "条");


        }
        public static List<long> checkReply(long group,string message) {
            //return false;
            bool flag = false;
            List<long> qqs = new List<long>();
            List<string> msgs = new List<string>();
            if (!GroupData.message.ContainsKey(group)) {//该群没有消息
                log("没有产生消息");
                return null;
            }
            Dictionary<int, Dictionary<string, string>> liveData = GroupData.message[group];

            if (maxSize> liveData.Count) {
                log("长度不足");
                return null;
            }

            log("------------------start----------------");

            




            string err = null;
            try {
                string tmpMsgValue = null;//按照第一条数据进行判断复读

                //判断是否3条数据全部相同qq相同信息发送
                

                //判断打断复读
                int k = 0;

                string t_s = null;
                foreach (var item in liveData) {
                    k++;
                    Dictionary<string, string> data = item.Value;
                    long qq = long.Parse(data["qq"].ToString());
                    string msg = data["msg"];
                   
                    string tmpMsg = String.Format("{0}-{1}", qq, msg);
                    log(tmpMsg);
                    if (t_s==null) {
                        t_s = tmpMsg;
                    }
                    if (!t_s.Equals(tmpMsg)) {//被打断
                        log("发现不同的数据");
                        break;
                    }
                    if (maxSize== k) {
                        //数据相同
                        throw new ReReplyException(Conf.getVar("复读机"));
                    }

                }


                k = 0;
                foreach (var item in liveData) {
                    k++;
                    Dictionary<string, string> data = item.Value;
                    int msgid = item.Key;
                    long qq = long.Parse(data["qq"].ToString());
                    string msg = data["msg"];
                    //通过取第一条msg作为验证
                    if (tmpMsgValue==null) {
                        tmpMsgValue = msg;
                    }
                    
                    

                    //1.验证是否存在不同的词条
                    if (!msg.Equals(tmpMsgValue)) {
                        if (err==null) {
                            err = "存在不相同msg:" + msg;
                            if (k == liveData.Count) {
                                //被打断复读
                                err = "存在不相同msg:" + msg+",【被打断复读!】";

                                GroupData.message[group].Clear();
                                throw new ReReplyException(Conf.getVar("打断复读"));
                            }
                        }
                        //throw new Exception("存在不相同msg:" + msg);
                    }




                    //2.验证是否指定条数内是否存在相同qq号码

                    if (qqs.Contains(qq)) {
                        //存在相同qq,则该组复读姬无效
                        if (err == null) {
                            err = "存在相同QQ:" + qq;

                        }
                        //throw new Exception("存在相同QQ:" + qq);
                    } else {
                        qqs.Add(qq);
                    }






                    log(qq + "=>" + msg);
                }
            }catch(ReReplyException e) {
                log("--------------无效的词组-----------------");
                log("--------------"+e.Message+"-------------------");
                log("------------------end----------------");
                throw e;
                //return null;
            }
            if (err!=null) {
                log("--------------无效的词组-----------------");
                log("--------------" + err + "-------------------");
                log("------------------end----------------");
                return null;
            }
            log("------------------end----------------");
            log("--------------------符合复读姬---------------");
            //清空词条
            GroupData.message[group].Clear();
            return qqs;

            //    if (qqs.Contains(qq)) {
            //        flag = true;//存在
            //        //CQ.SendPrivateMessage(2289453456, "出现相同QQ");
            //        log("出现相同QQ");
            //        break;//直接跳出
            //    } else {
            //        qqs.Add(qq);
            //    }


            //    if (!message.Equals(msg)) {
            //        log("出现不同的词条"+message+"---"+msg);
            //        flag = true;
            //        break;
            //    }
            //}

            //log(liveData.Count + ">=" + GroupData.maxSize);
            //if (liveData.Count>=GroupData.maxSize) {//消息足够才允许回复
            //    if (flag == true) {
            //        return null;
            //    } else {
            //        log("复读!清空词条");
            //        GroupData.message.Remove(group);
            //        GroupData.message.Add(group, new Dictionary<int, Dictionary<string, string>>());

            //        return qqs;
            //    }

            //} else {
            //    return null;
            //}

        }

        public static void log(string msg) {
            //Log.addLog("复读姬", msg);
            //CQ.SendPrivateMessage(2289453456, msg);
        }

    }



}
