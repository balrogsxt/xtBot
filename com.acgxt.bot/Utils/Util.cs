using com.acgxt.bot.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace com.acgxt.cqp.cs.Utils {
    public class Util {
        
        public static bool isUrl(string str) {
            try {
                string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(str, Url);
            } catch (Exception ex) {
                return false;
            }
        }
        public static string getRandomName(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom) {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++) {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        public static bool saveBitFile(WebResponse response, string FileName) {
            bool Value = true;
            byte[] buffer = new byte[1024];

            try {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                Stream outStream = System.IO.File.Create(FileName);
                Stream inStream = response.GetResponseStream();

                int l;
                do {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            } catch {
                Value = false;
            }
            return Value;
        }

        public static string downloadImageFile(string cqpCode) {
            string httpUrl = CQ.getImageUrl(cqpCode);
            if (httpUrl=="null") {
                return "null";
            }

            var reg = @"^\[CQ:image,file=([A-Z0-9]+).(png|jpg|bmp|jpeg|gif)\]$";
            MatchCollection mc = Regex.Matches(cqpCode, reg);
            string fileName = null;
            foreach (Match m in mc) {
                fileName = m.Groups[1] + "." + m.Groups[2];
            }
            if (fileName==null) {
                return "null";
            }



            string tmpImages = Path.Combine(Directory.GetCurrentDirectory(),"tmpImages");
            if (!Directory.Exists(tmpImages)) {
                Directory.CreateDirectory(tmpImages);
            }
            string fileAbsPath = Path.Combine(tmpImages, fileName);
            if (File.Exists(fileAbsPath)) {
                return fileAbsPath;
            }
            WebClient wc = new WebClient();

            wc.DownloadFile(httpUrl, fileAbsPath);

            return fileAbsPath;
        }
        public static string md5(string value) {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }
        public static string getFileMD5Hash(string filePath) {
            try {
                FileStream file = new FileStream(filePath, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                return null;
            }
        }
        public static string httpGet(string url) {
            WebClient wc = new WebClient();
            string value = Encoding.UTF8.GetString(wc.DownloadData(url));
            return value;
        }
        public static string httpGet(string url, int timeout) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.UserAgent = null;
                if (timeout != 0) {
                    request.Timeout = timeout;

                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            } catch (Exception e) {
                Log.addLog("GET请求发生异常:" + e.Message);
                return "NULL";
            }
        }

        public static bool isJson(string json) {
            try {
                JObject obj = JObject.Parse(json);
                return true;
            } catch (JsonReaderException e) {
                return false;
            } catch (Exception e) {
                return false;
            }
        }
        public static bool isJsonArray(string json) {
            try {
                JArray obj = JArray.Parse(json);
                return true;
            } catch (JsonReaderException e) {
                return false;
            } catch (Exception e) {
                return false;
            }
        }
        public static string getJsonParseError(string json) {
            try {
                JObject obj = JObject.Parse(json);
                return "";
            } catch (JsonReaderException e) {
                return e.Message;
            } catch (Exception e) {
                return e.Message;
            }
        }
  
        public static bool xsd_bfb(double val) {
            Random r = new Random();
            //大于100
            if (val>=100) {
                return true;
            }
            //0几率
            if (val==0) {
                return false;
            }
            //100以内
            if (100>val&&val>=1) {
                int data = r.Next(1, 101);
                if (val>=data) {
                    return true;
                } else {
                    return false;
                }
            }
            //小数点
            if (1>val) {
                int baseNum = int.Parse(Math.Floor(100 / val).ToString());
                int rand = r.Next(1,baseNum+1);
                if (rand==1) {
                    return true;
                }
            }
            return false;
        }
        public static bool bfb(int val) {
            Random r = new Random();
            var rand = r.Next(1, 101);
            //CQ.SendGroupMessage(550505327);
            if (val>=rand) {
                return true;
            } else {
                return false;
            }

        }
        public static string httpRequest(string url) {
            return httpRequest(url, "POST", null, "UTF-8", 10000);
        }
        public static string httpRequest(string url, string method) {
            return httpRequest(url, method, null, "UTF-8", 10000);
        }
        public static string httpRequest(string url, string method, string param) {
            return httpRequest(url, method, param, "UTF-8", 10000);
        }
        public static string httpRequest(string url, string method, string param, string encoding) {
            return httpRequest(url, method, param, encoding, 10000);
        }
        public static string httpRequest(string url, string method, string param, string encoding, int timeout) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = timeout;
            request.AllowAutoRedirect = true;
            request.Proxy = null;
            StreamWriter requestStream = null;
            WebResponse response = null;
            string responseStr = null;
            try {
                requestStream = new StreamWriter(request.GetRequestStream());
                if (param != null) {
                    requestStream.Write(param);
                }
                requestStream.Close();
                response = request.GetResponse();

                if (response != null) {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                    responseStr = sr.ReadToEnd();
                    sr.Close();
                }

            } catch (Exception e) {
                Console.WriteLine(e.Message);
            } finally {
                response = null;
                requestStream = null;
                request = null;
            }
            return responseStr;
        }


        //清除空格
        public static string clearEmpty(string text) {
            text = text.Replace(" ", "").Trim();
            text = text.Replace("\r\n", "").Trim();
            return text;
        }
        //检查是否为空
        public static bool checkEmpty(object data) {
            try {
                if (data==null) {
                    return true;
                }
                string text = data.ToString();
                //基础检查
                if (text.Length == 0 || text == "") {
                    return true;
                }
                if (text.Trim().Length == 0 || text.Trim() == "") {
                    return true;
                }
                //高级检查
                if (text.Replace(" ", "").Trim() == "" || text.Replace("\r\n", "").Trim() == "") {
                    return true;
                }

                return false;
            } catch (Exception e) {
                return true;
            }
        }
        public static long getTime() {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }




    }
}
