using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    //文件日志系统
    class Log {
        private static string liveDir = Directory.GetCurrentDirectory();
        private static string dataDir = Path.Combine(liveDir, "xtlogs");



        public static void info(string logs) {
            Log.addLog("INFO", "NONE", "NONE", logs);
        }
        public static void warning(string logs) {
            Log.addLog("WARNING", "NONE", "NONE", logs);
        }
        public static void error(string logs) {
            Log.addLog("ERROR", "NONE", "NONE", logs);

        }

        public static void addLog(string logs) {
            Log.addLog("INFO", "NONE", "NONE", logs);
        }
        public static void addLog(string type,string logs) {
            Log.addLog(type, "NONE", "NONE", logs);
        }
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="group">来自群</param>
        /// <param name="qq">发言用户</param>
        /// <param name="logs">日志内容</param>
        public static void addLog(string type,string group,string qq,string logs) {
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string filePath = Path.Combine(dataDir, fileName);
            string data = "";

            if (group != "NONE") {
                group = "[" + group + "]";
            } else {
                group = "";
            }
            if (qq != "NONE") {
                qq = "[" + qq + "]";
            } else {
                qq = "";
            }
            string type2 = type;
            if (type=="INFO") {
                type = "";
            } else {
                type = "[" + type + "]";
            }

            logs = String.Format("[{5}][{1}]{0}{2}{3}{4}", type ,DateTime.Now.ToString("HH:mm:ss"),group,qq, logs,type2);
            if (File.Exists(filePath)) {
                data = readFile(filePath);
                data += logs + "\r\n";
            } else {
                data = logs;
            }
            writeFile(filePath, data);
        }
        private static void writeFile(string path, string value) {
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
                data += line + "\r\n";
            }
            sr.Close();
            return data;
        }


    }
}
