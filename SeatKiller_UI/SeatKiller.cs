using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
        private static string history_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/history/1/";  // 预约历史记录API（拼接历史记录个数）
        private static string startTime_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/startTimesForSeat/";  // 座位开始时间API（拼接roomId+date）
        private static string endTime_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/endTimesForSeat/";  // 座位结束时间API（拼接roomId+date+startTime）
        private static string book_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/freeBook";  // 座位预约API
        private static string cancel_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/cancel/";  // 取消预约API（拼接预约ID）
        private static string stop_url = "https://seat.lib.whu.edu.cn:8443/rest/v2/stop";  // 座位释放API

        // 已预先爬取的roomId
        public static string[] xt = { "6", "7", "8", "9", "10", "11", "12", "16", "4", "5", "14", "15" };
        public static string[] xt_less = { "6", "7", "8", "9", "10", "11", "12", "16" };
        public static string[] gt = { "19", "29", "31", "32", "33", "34", "35", "37", "38" };
        public static string[] yt = { "20", "21", "23", "24", "26", "27" };
        public static string[] zt = { "39", "40", "51", "52", "56", "59", "60", "61", "62", "65", "66" };

        public static ArrayList freeSeats = new ArrayList();
        private static ArrayList startTimes = new ArrayList();
        private static ArrayList endTimes = new ArrayList();
        public static string token = "";
        public static string to_addr, res_id, username = "", password = "", name = "unknown", last_login_time = "unknown", state = "unknown", violationCount = "unknown";
        public static bool check_in, exchange = false;

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

        public static bool Wait(string hour, string minute, string second, bool enter = true)
        {
            DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + hour + ":" + minute + ":" + second);
            if (DateTime.Compare(DateTime.Now, time) > 0)
            {
                time.AddDays(1);
            }
            string originalText = Config.config.textBox2.Text;
            while (true)
            {
                TimeSpan delta = time.Subtract(DateTime.Now);
                Config.config.textBox2.Text = originalText;
                if (enter)
                {
                    Config.config.textBox2.AppendText("\r\n\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n");
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n");
                }
                if(delta.TotalSeconds<0)
                {
                    break;
                }
                Thread.Sleep(200);
            }
            return true;
        }

        public static string GetToken(bool test = false)
        {
            string url = login_url + "?username=" + username + "&password=" + password;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 3000;

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

        public static bool CheckResInf(bool alert = true)
        {
            string url = history_url + "30";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 2000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                string[] status = { "RESERVE", "CHECK_IN", "AWAY" };
                if (jObject["status"].ToString() == "success")
                {
                    foreach (JToken res in jObject["data"]["reservations"])
                    {
                        if (status.Contains(res["stat"].ToString()))
                        {
                            res_id = res["id"].ToString();
                            if (alert)
                            {
                                Reservation reservation = new Reservation();
                                reservation.label2.Text = "ID: " + res["id"] + "\r\n时间: " + res["date"] + " " + res["begin"] + "~" + res["end"];
                                switch (res["stat"].ToString())
                                {
                                    case "RESERVE":
                                        reservation.label2.Text = reservation.label2.Text + "\r\n状态: 预约";
                                        reservation.label3.Text = "是否取消此预约？（若不取消可自动改签座位）";
                                        check_in = false;
                                        break;
                                    case "CHECK_IN":
                                        if (res["awayEnd"].ToString() != "")
                                        {
                                            reservation.label2.Text = reservation.label2.Text + "\r\n暂离时间: " + res["awayBegin"].ToString() + "~" + res["awayEnd"].ToString();
                                            reservation.label3.Location = new Point(110, 253);
                                        }
                                        reservation.label2.Text = reservation.label2.Text + "\r\n状态: 已签到";
                                        reservation.label3.Text = "是否释放此座位？（若不释放可自动改签座位）";
                                        check_in = true;
                                        break;
                                    default:
                                        reservation.label2.Text = reservation.label2.Text + "\r\n暂离时间: " + res["awayBegin"].ToString();
                                        reservation.label2.Text = reservation.label2.Text + "\r\n状态: 暂离";
                                        reservation.label3.Text = "是否释放此座位？（若不释放可自动改签座位）";
                                        check_in = true;
                                        break;
                                }
                                reservation.label2.Text = reservation.label2.Text + "\r\n地址: " + res["loc"].ToString() + "\r\n-----------------------------------------------------------------";
                                reservation.Show();
                            }
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
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
                    Config.config.textBox2.AppendText("\r\n");
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

        public static bool CancelReservation(string id, bool mute = false)
        {
            string url = cancel_url + id;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            if (!mute)
            {
                Config.config.textBox2.AppendText("\r\nTry cancelling reservation.....Status : ");
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (!mute)
                {
                    Config.config.textBox2.AppendText(jObject["status"].ToString());
                }
                if (jObject["status"].ToString() == "success")
                {
                    return true;
                }
                else
                {
                    if (!mute)
                    {
                        Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    }
                    return false;
                }
            }
            catch
            {
                if (!mute)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return false;
            }
        }

        public static bool StopUsing(bool mute = false)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(stop_url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            if (!mute)
            {
                Config.config.textBox2.AppendText("\r\nTry releasing seat.....Status : ");
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                if (!mute)
                {
                    Config.config.textBox2.AppendText(jObject["status"].ToString());
                }
                if (jObject["status"].ToString() == "success")
                {
                    return true;
                }
                else
                {
                    if (!mute)
                    {
                        Config.config.textBox2.AppendText("\r\n" + jObject.ToString());
                    }
                    return false;
                }
            }
            catch
            {
                if (!mute)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
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
                if (jObject["data"]["seats"].ToString() != "{}")
                {
                    Config.config.textBox2.AppendText("success");
                    JToken seats = jObject["data"]["seats"];
                    foreach (var num in seats)
                    {
                        freeSeats.Add(num.First["id"].ToString());
                    }
                    return "Success";
                }
                else
                {
                    Config.config.textBox2.AppendText("fail");
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
                    exchange = true;
                    PrintBookInf(jObject);
                    if (Config.config.checkBox2.Checked)
                    {
                        jObject.Add("username", username);
                        jObject.Add("name", name);
                        jObject.Add("client", "C#");
                        SendMail(jObject.ToString(), to_addr);
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

        public static void PrintBookInf(JObject jObject)
        {
            Config.config.textBox2.AppendText("\r\n--------------------------座位预约成功--------------------------");
            Config.config.textBox2.AppendText("\r\nID：" + jObject["data"]["id"]);
            Config.config.textBox2.AppendText("\r\n凭证号码：" + jObject["data"]["receipt"]);
            Config.config.textBox2.AppendText("\r\n时间：" + jObject["data"]["onDate"] + " " + jObject["data"]["begin"] + "~" + jObject["data"]["end"]);
            if (jObject["data"]["checkedIn"].ToString() == "False")
                Config.config.textBox2.AppendText("\r\n状态：预约");
            else
                Config.config.textBox2.AppendText("\r\n状态：已签到");
            Config.config.textBox2.AppendText("\r\n地址：" + jObject["data"]["location"]);
            Config.config.textBox2.AppendText("\r\n------------------------------------------------------------------");
        }

        public static void SendMail(string json, string to_addr)
        {
            if (Regex.IsMatch(to_addr, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
            {
                Config.config.textBox2.AppendText("\r\n\r\n尝试连接邮件服务器");
                byte[] data = new byte[5];
                Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse("120.79.81.183");
                IPEndPoint point = new IPEndPoint(ip, 5210);

                socketClient.Connect(point);
                socketClient.Receive(data);
                string stringData = Encoding.UTF8.GetString(data);

                if (stringData == "Hello")
                {
                    Config.config.textBox2.AppendText("\r\n连接成功");

                    data = new byte[128];
                    socketClient.Send(Encoding.UTF8.GetBytes("json" + json));
                    socketClient.Receive(data);
                    Config.config.textBox2.AppendText("\r\n" + Encoding.UTF8.GetString(data));

                    data = new byte[128];
                    socketClient.Send(Encoding.UTF8.GetBytes(to_addr));
                    socketClient.Receive(data);
                    Config.config.textBox2.AppendText("\r\n" + Encoding.UTF8.GetString(data));

                    Config.config.textBox2.AppendText("\r\n正在发送邮件提醒至\"" + to_addr + "\"");
                    data = new byte[7];
                    socketClient.Send(Encoding.UTF8.GetBytes("SendMail"));
                    socketClient.Receive(data);
                    stringData = Encoding.UTF8.GetString(data);
                    if (stringData == "success")
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n邮件提醒发送成功，若接收不到提醒，请将\"seatkiller@outlook.com\"添加至邮箱白名单");
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n邮箱提醒发送失败");
                    }
                    socketClient.Send(Encoding.UTF8.GetBytes("exit"));
                    socketClient.Close();
                }
            }
            else if (to_addr == "")
            {
                Config.config.textBox2.AppendText("\r\n\r\n邮箱地址为空，无法发送邮件");
            }
            else
            {
                Config.config.textBox2.AppendText("\r\n\r\n邮箱地址输入错误，无法发送邮件");
            }
        }

        public static bool CheckStartTime(string seatId, string date, string startTime)
        {
            string url = startTime_url + seatId + "/" + date;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry checking startTimes of seat No." + seatId + ".....Status : ");
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
                    startTimes.Clear();
                    JToken getStartTimes = jObject["data"]["startTimes"];
                    foreach (var time in getStartTimes)
                    {
                        startTimes.Add((time["id"].ToString()));
                    }

                    if (startTimes.Contains(startTime))
                    {
                        Config.config.textBox2.AppendText("success");
                        return true;
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("fail");
                        return false;
                    }
                }
                else
                {
                    Config.config.textBox2.AppendText("fail");
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static bool CheckEndTime(string seatId, string date, string startTime, string endTime)
        {
            string url = endTime_url + seatId + "/" + date + "/" + startTime;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 5000;

            Config.config.textBox2.AppendText("\r\nTry checking endTimes of seat No." + seatId + ".....Status : ");
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
                    endTimes.Clear();
                    JToken getEndTimes = jObject["data"]["endTimes"];
                    foreach (var time in getEndTimes)
                    {
                        endTimes.Add((time["id"].ToString()));
                    }

                    if (endTimes.Contains(endTime))
                    {
                        Config.config.textBox2.AppendText("success");
                        return true;
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("fail");
                        return false;
                    }
                }
                else
                {
                    Config.config.textBox2.AppendText("fail");
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static bool Loop(string buildingId, string[] rooms, string startTime, string endTime, string roomId = "0", string seatId = "0")
        {
            Config.config.textBox2.AppendText("\r\n\r\n--------------------------进入捡漏模式--------------------------\r\n");
            if (roomId == "0")
            {
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 01:00:00")) < 0)
                    Wait("01", "00", "00", false);
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                GetRooms(buildingId);
                while (true)
                {
                    freeSeats.Clear();
                    if (GetToken() == "Success")
                    {
                        foreach (var room in rooms)
                        {
                            if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                Thread.Sleep(30000);
                            }
                        }

                        foreach (var freeSeatId in freeSeats)
                        {
                            switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                            {
                                case "Success":
                                    Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                                    Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                                    return true;
                                case "Failed":
                                    Thread.Sleep(2000);
                                    continue;
                                case "Connection lost":
                                    Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续预约空位\r\n");
                                    Thread.Sleep(30000);
                                    continue;
                            }
                        }

                        DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00");
                        if (DateTime.Compare(DateTime.Now, time) > 0)
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n捡漏失败，超出运行时间\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                            return false;
                        }

                        TimeSpan delta = time.Subtract(DateTime.Now);
                        Config.config.textBox2.AppendText("\r\n\r\n循环结束，3秒后进入下一个循环，运行时间剩余" + delta.TotalSeconds.ToString() + "秒\r\n");
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n获取token失败，请检查网络后重试\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                        return false;
                    }
                }
            }
            else if (seatId == "0")
            {
                Config.config.textBox2.AppendText("\r\n正在监控区域，ID: " + roomId + "\r\n");
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                while (true)
                {
                    freeSeats.Clear();
                    if (SearchFreeSeat(buildingId, roomId, date, startTime, endTime) == "Success")
                    {
                        foreach (var freeSeatId in freeSeats)
                        {
                            switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                            {
                                case "Success":
                                    Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                                    Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                                    return true;
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

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n捡漏失败，超出运行时间\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                        return false;
                    }
                    Thread.Sleep(2000);
                }
            }
            else
            {
                Config.config.textBox2.AppendText("\r\n正在监控座位，ID: " + seatId + "\r\n");
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                while (true)
                {
                    if (BookSeat(seatId, date, startTime, endTime) == "Success")
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                        return true;
                    }

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n捡漏失败，超出运行时间\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出捡漏模式--------------------------");
                        return false;
                    }
                    Thread.Sleep(2000);
                }
            }
        }

        public static bool ExchangeLoop(string buildingId, string[] rooms, string startTime, string endTime, string roomId = "0", string seatId = "0")
        {
            Config.config.textBox2.AppendText("\r\n\r\n--------------------------进入改签模式--------------------------\r\n");
            bool cancelled = false;
            GetRooms(buildingId);
            if (roomId == "0")
            {
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 01:00:00")) < 0)
                    Wait("01", "00", "00", false);
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                while (true)
                {
                    freeSeats.Clear();
                    if (GetToken() == "Success")
                    {
                        foreach (var room in rooms)
                        {
                            if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                Thread.Sleep(30000);
                            }
                        }

                        foreach (var freeSeatId in freeSeats)
                        {
                            if (!cancelled)
                            {
                                if (CheckStartTime(freeSeatId.ToString(), date, startTime) & CheckEndTime(freeSeatId.ToString(), date, startTime, endTime))
                                {
                                    if (check_in)
                                    {
                                        if (StopUsing())
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (CancelReservation(res_id))
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                            return false;
                                        }
                                    }
                                }
                            }

                            switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                            {
                                case "Success":
                                    Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                                    Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                    return true;
                                case "Failed":
                                    Thread.Sleep(2000);
                                    continue;
                                case "Connection lost":
                                    Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续预约空位\r\n");
                                    Thread.Sleep(30000);
                                    continue;
                            }
                        }

                        DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00");
                        if (DateTime.Compare(DateTime.Now, time) > 0)
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n改签失败，超出运行时间\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                            return false;
                        }

                        TimeSpan delta = time.Subtract(DateTime.Now);
                        Config.config.textBox2.AppendText("\r\n\r\n循环结束，3秒后进入下一个循环，运行时间剩余" + delta.TotalSeconds.ToString() + "秒\r\n");
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n获取token失败，请检查网络后重试\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                        return false;
                    }
                }
            }
            else if (seatId == "0")
            {
                Config.config.textBox2.AppendText("\r\n正在监控区域，ID: " + roomId + "\r\n");
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                while (true)
                {
                    freeSeats.Clear();
                    if (SearchFreeSeat(buildingId, roomId, date, startTime, endTime) == "Success")
                    {
                        foreach (var freeSeatId in freeSeats)
                        {
                            if (CheckStartTime(freeSeatId.ToString(), date, startTime) & CheckEndTime(freeSeatId.ToString(), date, startTime, endTime))
                            {
                                if (!cancelled)
                                {
                                    if (check_in)
                                    {
                                        if (StopUsing())
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (CancelReservation(res_id))
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                            return false;
                                        }
                                    }
                                }

                                switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                                {
                                    case "Success":
                                        Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                                        return true;
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
                        Thread.Sleep(2000);
                    }

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n改签失败，超出运行时间\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                        return false;
                    }
                    Thread.Sleep(2000);
                }
            }
            else
            {
                Config.config.textBox2.AppendText("\r\n正在监控座位，ID: " + seatId + "\r\n");
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                while (true)
                {
                    if (CheckStartTime(seatId, date, startTime) & CheckEndTime(seatId, date, startTime, endTime))
                    {
                        break;
                    }

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 20:00:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n改签失败，超出运行时间\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                        return false;
                    }

                    Thread.Sleep(2000);
                }

                if (check_in)
                {
                    if (StopUsing())
                    {
                        if (BookSeat(seatId, date, startTime, endTime) == "Success")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                            return true;
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n改签失败，原座位已丢失\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                            return false;
                        }
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，请稍后重试\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                        return false;
                    }
                }
                else
                {
                    if (CancelReservation(res_id))
                    {
                        if (BookSeat(seatId, date, startTime, endTime) == "Success")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                            return true;
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n改签失败，原座位已丢失\r\n");
                            Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                            return false;
                        }
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，请稍后重试\r\n");
                        Config.config.textBox2.AppendText("\r\n--------------------------退出改签模式--------------------------");
                        return false;
                    }
                }
            }
        }
    }
}
