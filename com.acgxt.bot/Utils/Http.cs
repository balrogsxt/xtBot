using com.acgxt.cqp.cs.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.acgxt.bot.Utils {
    class Http {
        private string cookie = String.Empty;//储存的Cookie
        private string url = String.Empty;//请求的url
        private string result = String.Empty;//请求结果
        private string encoding = "UTF-8";//编码

        public Http(string url) {
            this.url = url;
        }
        public void setEncoding(string encoding) {
            this.encoding = encoding;
        }
        public void setCookie(string cookie) {
            this.cookie = cookie;
        }
        public void getImage(string method, string param, string imageSaveFile) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.url);
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(this.url), this.cookie);

            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (!Util.checkEmpty(param)) {//设置请求参数
                StreamWriter requestStream = new StreamWriter(request.GetRequestStream());
                requestStream.Write(param);
                requestStream.Close();
            }

            response = (HttpWebResponse)request.GetResponse();
            this.cookie = response.Headers["Set-Cookie"];//获取Cookie

            Stream rs = response.GetResponseStream();
            Image.FromStream(rs).Save(imageSaveFile);
            rs.Close();
        }
        public string getResult() {
            return this.getResult("GET", "");
        }
        public string getResult(string method, string param) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.url);
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(this.url), this.cookie);

            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            //request.Referer = "https://www.neetvideo.com/register";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; Alexa Toolbar)";

            if (!Util.checkEmpty(param)) {//设置请求参数
                StreamWriter requestStream = new StreamWriter(request.GetRequestStream());
                requestStream.Write(param);
                requestStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            response = (HttpWebResponse)request.GetResponse();
            this.cookie = response.Headers["Set-Cookie"];//获取Cookie


            Stream rs = response.GetResponseStream();
            StreamReader sr = new StreamReader(rs, Encoding.GetEncoding(encoding));
            this.result = sr.ReadToEnd();
            sr.Close();
            rs.Close();
            return this.result;
        }

        public string getCookie() {
            return this.cookie;
        }

    }
}
