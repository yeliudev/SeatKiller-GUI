using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
        public static string[] xt = { "6", "7", "8", "9", "10", "11", "12", "4", "5" };
        public static string[] gt = { "19", "29", "31", "32", "33", "34", "35", "37", "38" };
        public static string[] yt = { "20", "21", "23", "24", "26", "27" };
        public static string[] zt = { "39", "40", "51", "52", "56", "59", "60", "61", "62", "65", "66" };

        public static ArrayList freeSeats = new ArrayList();
        public static string token = "75PLJJO8PV12084027";  // 预先移动端抓包获取
        public static string to_addr, username = "", password = "", name = "unknown", last_login_time = "unknown", state = "unknown", violationCount = "unknown";

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

        public static bool Wait(string hour, string minute, string second, bool nextDay = false)
        {
            DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + hour + ":" + minute + ":" + second);
            if (nextDay)
            {
                time.AddDays(1);
            }
            if (DateTime.Compare(DateTime.Now, time) > 0)
            {
                time.AddDays(1);
            }
            TimeSpan delta = time.Subtract(DateTime.Now);
            Config.config.textBox2.AppendText("\r\n\r\n正在等待系统开放，剩余" + delta.TotalSeconds.ToString() + "秒\r\n");
            Thread.Sleep((int)delta.TotalMilliseconds);
            return true;
        }

        public static string GetToken(bool test = false)
        {
            string url = login_url + "?username=" + username + "&password=" + password;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            if (!test)
            {
                Config.config.textBox2.AppendText("\r\nTry getting token.....Status : ");
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (!test)
                {
                    Config.config.textBox2.AppendText(jObject["status"].ToString());
                }
                if (jObject["status"].ToString() == "success")
                {
                    token = jObject["data"]["token"].ToString();
                    return "Success";
                }
                else
                {
                    if (!test)
                    {
                        Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    }
                    return jObject["message"].ToString();
                }
            }
            catch
            {
                if (!test)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return "Connection lost";
            }
        }

        public static bool GetUsrInf()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(usr_url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry getting user information.....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString());
                if (jObject["status"].ToString() == "success")
                {
                    name = jObject["data"]["name"].ToString();
                    last_login_time = jObject["data"]["lastLogin"].ToString();
                    if (jObject["data"]["checkedIn"].ToString() == "true")
                    {
                        state = "已进入" + jObject["data"]["lastInBuildingName"].ToString();
                    }
                    else
                    {
                        state = "未入馆";
                    }
                    violationCount = jObject["data"]["violationCount"].ToString();
                    return true;
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static bool GetBuildings()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(filters_url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry getting building information.....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString());
                if (jObject["status"].ToString() == "success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static bool GetRooms(string buildingId)
        {
            string url = stats_url + buildingId;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry getting room information.....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString());
                if (jObject["status"].ToString() == "success")
                {
                    JToken jToken = jObject["data"];
                    Config.config.textBox2.AppendText("\r\n\r\n当前座位状态：");
                    foreach (var room in jToken)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n" + room["room"].ToString() + "\r\n楼层：" + room["floor"].ToString() + "\r\n总座位数：" + room["totalSeats"].ToString() + "\r\n已预约：" + room["reserved"].ToString() + "\r\n正在使用：" + room["inUse"].ToString() + "\r\n暂离：" + room["away"].ToString() + "\r\n空闲：" + room["free"].ToString());
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
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
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry getting seat information in room " + roomId + ".....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString());
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
                    NewComparer newComparer = new NewComparer();
                    seats.Sort(newComparer);
                    return true;
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static string SearchFreeSeat(string buildingId, string roomId, string date, string startTime, string endTime)
        {
            string url = search_url + date + "/" + startTime + "/" + endTime;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("{0}={1}", "t", "1");
            buffer.AppendFormat("&{0}={1}", "roomId", roomId);
            buffer.AppendFormat("&{0}={1}", "buildingId", buildingId);
            buffer.AppendFormat("&{0}={1}", "batch", "9999");
            buffer.AppendFormat("&{0}={1}", "page", "1");
            buffer.AppendFormat("&{0}={1}", "t2", "2");
            byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);

            Config.config.textBox2.AppendText("\r\nTry searching for free seats in room " + roomId + ".....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString());
                if (jObject["data"]["seats"].ToString() != "")
                {
                    JToken seats = jObject["data"]["seats"];
                    foreach (var num in seats)
                    {
                        freeSeats.Add(num.First["id"].ToString());
                    }
                    return "Success";
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    return "Failed";
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return "Connection lost";
            }
        }

        public static string BookSeat(string seatId, string date, string startTime, string endTime)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(book_url);
            request.Method = "POST";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("{0}={1}", "t", "1");
            buffer.AppendFormat("&{0}={1}", "startTime", startTime);
            buffer.AppendFormat("&{0}={1}", "endTime", endTime);
            buffer.AppendFormat("&{0}={1}", "seat", seatId);
            buffer.AppendFormat("&{0}={1}", "date", date);
            buffer.AppendFormat("&{0}={1}", "t2", "2");
            byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);

            Config.config.textBox2.AppendText("\r\n\r\nTry booking seat.....Status : ");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                Config.config.textBox2.AppendText(jObject["status"].ToString() + "\r\n");
                if (jObject["status"].ToString() == "success")
                {
                    return "Success";
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    return "Failed";
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost\r\n");
                return "Connection lost";
            }
        }

        public static bool Loop(string buildingId, string[] rooms, string startTime, string endTime)
        {
            Config.config.textBox2.AppendText("\r\n\r\n------------------------------进入捡漏模式------------------------------\r\n");
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            while (true)
            {
                freeSeats.Clear();
                if (GetToken() == "Success")
                {
                    foreach (var roomId in rooms)
                    {
                        if (SearchFreeSeat(buildingId, roomId, date, startTime, endTime) == "Connection lost")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                            Thread.Sleep(30000);
                        }
                    }

                    foreach (var freeSeatId in freeSeats)
                    {
                        switch(BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                        {
                            case "Success":
                                Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                                Config.config.textBox2.AppendText("\r\n\r\n------------------------------进入捡漏模式------------------------------\r\n");
                                break;
                            case "Failed":
                                Thread.Sleep(2000);
                                continue;
                            case "Connection lost":
                                Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续预约空位\r\n");
                                Thread.Sleep(30000);
                                continue;
                        }
                    }
                }
            }
        }
    }
}
