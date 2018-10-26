using Baidu.Aip.ContentCensor;
using com.acgxt.bot.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.acgxt.cqp.cs.Utils {
    public class Conf {
        public static string liveDir = Directory.GetCurrentDirectory();
        public static string dataDir = Path.Combine(liveDir,"data");
        public static AntiPorn ap;
        public static ImageCensor ic;
        public static void baiduAiInit() {

            object appid = Conf.getConfig("global.config", "baiduappid");
            object apikey = Conf.getConfig("global.config", "baiduapikey");
            object secretkey = Conf.getConfig("global.config", "baidusecretkey");

            if (appid==null)appid = "";
            if (apikey == null) apikey = "";
            if (secretkey == null) secretkey = "";
            Log.addLog("appid=" + appid);
            Log.addLog("apikey=" + apikey);
            Log.addLog("secretkey=" + secretkey);



            var APP_ID = appid.ToString();
            var API_KEY = apikey.ToString();
            var SECRET_KEY = secretkey.ToString();

            Conf.ap = ap = new AntiPorn(API_KEY,SECRET_KEY);
            Conf.ic = new ImageCensor(API_KEY, SECRET_KEY);
            Log.addLog("初始化百度AI完成");

        }

        private static void autoCreateUserDataDir() {
            if (!Directory.Exists(dataDir)) {
                Directory.CreateDirectory(dataDir);
            }
        }
        /// <summary>
        /// 设置全局变量
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="varValue"></param>
        public static void setVar(string varName,string varValue) {
            Conf.setConfig("global.var", varName, varValue);
        }
        public static string getVar(string varName) {
            try {
                object obj = Conf.getConfig("global.var", varName);
                return obj.ToString();
            } catch (Exception e) {
                Log.warning("获取全局词条失败:" + e.Message);
                return "["+ varName + "]";
            }
        }
        public static string getVar(string varName,string def) {
            try {
                object obj = Conf.getConfig("global.var", varName);
                return obj.ToString();
            } catch (Exception e) {
                Log.warning("获取全局词条失败:" + e.Message);
                return def;
            }
        }

        private static string parsePath(string path) {
            string[] paths = path.Split('.');
            string file = dataDir;
            for (int i=0;i<paths.Length;i++) {
                if (i==paths.Length-1) {
                    file = Path.Combine(file, paths[i]+".json");
                    if (!File.Exists(file)) {
                        FileStream fs = File.Create(file);
                        fs.Close();
                    }
                } else {
                    file = Path.Combine(file, paths[i]);
                    if (!Directory.Exists(file)) {
                       DirectoryInfo di = Directory.CreateDirectory(file);
                    }
                }
            }
            return file;
        }
        public static string getStringConfig(string path, string key, string def) {
            object data = getConfig(path, key);
            if (data == null) {
                return def;
            }
            return data.ToString();
        }
        public static int getIntConfig(string path, string key, int def) {
            object data = getConfig(path, key);
            if (data == null) {
                return def;
            }
            try {
                return int.Parse(data.ToString());
            } catch (Exception e) {
                return def;
            }
        }
        public static long getLongConfig(string path, string key, long def) {
            object data = getConfig(path, key);
            if (data == null) {
                return def;
            }
            try {
                return long.Parse(data.ToString());
            } catch (Exception e) {
                return def;
            }
        }

        public static object getConfig(string path,string key) {
            autoCreateUserDataDir();
            string file = parsePath(path);
            string json = readFile(file);
            Dictionary<object, object> map = new Dictionary<object, object>();
            if (Util.isJson(json)) {
                JObject jo = JObject.Parse(json);
                JToken jt = jo;
                foreach (JProperty jp in jt) {
                    if (key!=null) {
                        if (key == jp.Name) {
                            return jp.Value;
                        }
                    } else {
                        map.Add(jp.Name,jp.Value);
                    }
                    
                }
            }
            if (key==null) {
                return map;
            } else {
                return null;
            }
        }
        public static void setConfig(string path, string key, object value) {
            autoCreateUserDataDir();
            string file = parsePath(path);
            string json = readFile(file);
            Dictionary<object, object> map = new Dictionary<object, object>();
            if (Util.isJson(json)) {
                JObject jo = JObject.Parse(json);

                JToken jt = jo;
                foreach (JProperty jp in jt) {
                    map.Add(jp.Name, jp.Value);
                }
            }
            if (map.ContainsKey(key)) {
                map.Remove(key);
            }

            map.Add(key, value);

            string data = JsonConvert.SerializeObject(map);
            writeFile(file, data);
        }
        public static void setConfig(string path,object value) {
            autoCreateUserDataDir();
            string file = parsePath(path);
            string data = JsonConvert.SerializeObject(value);
            writeFile(file, data);
        }
        private static void writeFile(string path,string value) {
            FileStream fs = new FileStream(path, FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(value);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        private static string readFile(string path) {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            string data = "";
            while ((line = sr.ReadLine()) != null) {
                data += line;
            }
            sr.Close();
            return data;
        }

    }
}
