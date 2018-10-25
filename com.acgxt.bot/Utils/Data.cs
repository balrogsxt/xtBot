using com.acgxt.bot.Utils;
using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace com.acgxt.cqp.cs.Utils {
    //临时储存数据消息
    class Data {

        
        public static Dictionary<int, string> list = new Dictionary<int, string>();
        public static IMahuaApi api = null;

        /// <summary>
        /// 通过message准确查询最后所属msgid
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static long getMsgid(string message,string group){
            if (Data.api.GetLoginQq()==null)return (-1);


            try
            {
                string liveDir = Directory.GetCurrentDirectory();
                string sqliteFile = Path.Combine(liveDir, @"data\" + Data.api.GetLoginQq() + @"\eventv2.db");
                if (!File.Exists(sqliteFile)) return (-2);


                SQLiteConnection conn = new SQLiteConnection();
                conn.ConnectionString = "Data Source=" + sqliteFile;
                conn.Open();
                string fieldGroup = "qq/group/" + group;
                string sql = "select * from event WHERE `group` = '"+fieldGroup+"' ORDER BY id desc LIMIT 0,5";
                //Log.addLog("撤回消息SQL:" + sql);
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;


                //是否是分享群
                long shareGroup = CQ.getShareGroup(message);
                if (shareGroup!=0) {
                    message = "[CQ:contact,id=qq/group/"+shareGroup+"]";
                }

                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    long msgid = reader.GetInt16(0);
                    string msg = reader.GetString(6);
                    if (msg.Equals(message))
                    {
                        //Log.addLog("GET_MSGID", group, "", "成功匹配MSG:[" + msg + "]->[" + message + "]");

                        if (!reader.IsClosed) reader.Close();
                        conn.Close();
                        return msgid;
                    } else {
                        //Log.addLog("GET_MSGID", group, "", "失败匹配MSG:[" + msg + "]->[" + message+"]");

                    }
                }
                Log.addLog("GET_MSGID",group,"","获取msgid失败SQL:" +sql);
                return 0;
            }catch(Exception e){
                Log.addLog("ERROR",group,"","撤回消息Error:" + e.Message);
                return (-10);
            }
        }







        
        public static void addMessage(int msgid,string msg) {
            Data.list.Add(msgid, msg);
        }

        

        public static bool checkRemsg(string msg) {







            return true;
        }








    }
}
