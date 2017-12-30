using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SeatKiller_UI
{
    public static class SeatKiller
    {
        private static string login_url = "https://seat.lib.whu.edu.cn:8443/rest/auth";  // 图书馆移动端登陆API
        private static string usr_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/user";  // 用户信息API
        private static string filters_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/free/filters";  // 分馆和区域信息API
        private static string stats_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/room/stats2/";  // 单一分馆区域信息API（拼接buildingId）
        private static string layout_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/room/layoutByDate/";  // 单一区域座位信息API（拼接roomId+date）
        private static string search_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/searchSeats/";  // 空位检索API（拼接date+startTime+endTime）
        private static string book_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/freeBook";  // 座位预约API

        // 已预先爬取的roomId
        private static string[] xt = { "6", "7", "8", "9", "10", "11", "12", "4", "5" };
        private static string[] gt = { "19", "29", "31", "32", "33", "34", "35", "37", "38" };
        private static string[] yt = { "20", "21", "23", "24", "26", "27" };
        private static string[] zt = { "39", "40", "51", "52", "56", "59", "60", "61", "62", "65", "66" };

        public static string[] freeSeats = { };
        public static string token = "75PLJJO8PV12084027";  // 预先移动端抓包获取
        public static string to_addr, username = "", password = "", name = "unknown", last_login_time = "unknown";

        private static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }

        public static void SetHeaderValues(HttpWebRequest request)
        {
            SetHeaderValue(request.Headers, "Host", "seat.lib.whu.edu.cn:8443");
            SetHeaderValue(request.Headers, "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            SetHeaderValue(request.Headers, "Connection", "keep-alive");
            SetHeaderValue(request.Headers, "Accept", "*/*");
            SetHeaderValue(request.Headers, "User-Agent", "doSingle/11 CFNetwork/893.14.2 Darwin/17.3.0");
            SetHeaderValue(request.Headers, "Accept-Language", "zh-cn");
            SetHeaderValue(request.Headers, "token", token);
            SetHeaderValue(request.Headers, "Accept-Encoding", "gzip, deflate");
        }

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        public static string GetToken()
        {
            string url = login_url + "?username=" + username + "&password=" + password;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (jObject["status"].ToString() == "success")
                {
                    token = jObject["data"]["token"].ToString();
                }
                return jObject["status"].ToString();
            }
            catch
            {
                return "Connection lost";
            }
        }

        public static bool GetUsrInf()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(usr_url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (jObject["status"].ToString() == "success")
                {
                    name = jObject["data"]["name"].ToString();
                    last_login_time = jObject["data"]["lastLogin"].ToString();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetSeats(string roomId, ArrayList seats)
        {
            string url = layout_url + roomId + "/" + DateTime.Now.ToString("yyyy-MM-dd");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (jObject["status"].ToString() == "success")
                {
                    JToken layout = jObject["data"]["layout"];
                    foreach (var num in layout)
                    {
                        if (num.First["type"].ToString() == "seat")
                        {
                            seats.Add(new DictionaryEntry(num.First["id"].ToString(), num.First["name"].ToString()));
                        }
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
