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
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public static class SeatKiller
    {
        private const string API_ROOT = "https://seat.lib.whu.edu.cn:8443/rest/";
        private const string API_V2_ROOT = "https://seat.lib.whu.edu.cn:8443/rest/v2/";
        private const string SERVER = "134.175.186.17";

        public static readonly string[] xt = { "6", "7", "8", "9", "10", "11", "12", "16", "4", "5", "14", "15" };
        public static readonly string[] xt_elite = { "6", "7", "8", "9", "10", "11", "16" };
        public static readonly string[] gt = { "19", "29", "31", "32", "33", "34", "35", "37", "38" };
        public static readonly string[] yt = { "20", "21", "23", "24", "26", "27" };
        public static readonly string[] zt = { "39", "40", "51", "52", "56", "59", "60", "61", "62", "65", "66" };

        public static ArrayList freeSeats = new ArrayList();
        private static ArrayList startTimes = new ArrayList(), endTimes = new ArrayList();
        public static string to_addr, res_id, username, password, newVersion, newVersionSize, updateInfo, downloadURL, status, bookedSeatId, historyDate, historyStartTime, historyEndTime, historyAwayStartTime, token, name, last_login_time, state, violationCount;
        public static bool checkedIn, reserving, onlyPower, onlyWindow, onlyComputer, exitFlag = true;
        public static DateTime time;

        private static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }

        private static void SetHeaderValues(HttpWebRequest request)
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

        private static JObject HttpGetRequest(string url, int timeout = 5000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = timeout;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));

            return JObject.Parse(streamReader.ReadToEnd());
        }

        private static JObject HttpPostRequest(string url, byte[] data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            SetHeaderValues(request);
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);
            request.Timeout = 5000;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));

            return JObject.Parse(streamReader.ReadToEnd());
        }

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        public static bool CheckUpdate()
        {
            string url = "https://api.github.com/repos/goolhanrry/Seatkiller_UI/releases/latest";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            SetHeaderValue(request.Headers, "User-Agent", "doSingle/11 CFNetwork/893.14.2 Darwin/17.3.0");
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            request.Timeout = 3000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader streamReader = new StreamReader(stream, encoding);
                string json = streamReader.ReadToEnd();
                JObject res = JObject.Parse(json);
                if (new Version(res["tag_name"].ToString().Substring(1)) > new Version(Application.ProductVersion))
                {
                    newVersion = res["tag_name"].ToString().Substring(1);
                    newVersionSize = (Convert.ToDouble(res["assets"].First["size"].ToString()) / (1024 * 1024)).ToString().Substring(0, 4) + " MB";
                    updateInfo = res["body"].ToString();
                    downloadURL = res["assets"].First["browser_download_url"].ToString();
                    return true;
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

        public static void Wait(string hour, string minute, string second, bool enter = true)
        {
            time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " " + hour + ":" + minute + ":" + second);
            if (DateTime.Compare(DateTime.Now, time) > 0)
            {
                time = time.AddDays(1);
            }
            Config.config.backgroundWorker1.RunWorkerAsync(enter);
            while (true)
            {
                TimeSpan delta = time.Subtract(DateTime.Now);
                if (delta.TotalSeconds < 0)
                {
                    Config.config.backgroundWorker1.CancelAsync();
                    while (true)
                    {
                        if (!Config.config.backgroundWorker1.IsBusy)
                        {
                            break;
                        }
                    }
                    break;
                }
                Thread.Sleep(5);
            }
            return;
        }

        public static string GetToken(bool alert = true)
        {
            string url = API_ROOT + "auth?username=" + username + "&password=" + password;

            if (alert)
            {
                Config.config.textBox2.AppendText("\r\nRequesting for token.....Status : ");
            }

            try
            {
                JObject res = HttpGetRequest(url, 3000);

                if (alert)
                {
                    Config.config.textBox2.AppendText(res["status"].ToString());
                }

                if (res["status"].ToString() == "success")
                {
                    token = res["data"]["token"].ToString();
                    return "Success";
                }
                else
                {
                    if (alert)
                    {
                        Config.config.textBox2.AppendText("\r\n" + res.ToString());
                    }
                    return res["message"].ToString();
                }
            }
            catch
            {
                if (alert)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return "Connection lost";
            }
        }

        public static bool CheckResInf(bool alert = true)
        {
            string url = API_V2_ROOT + "history/1/30";
            string[] probableStatus = { "RESERVE", "CHECK_IN", "AWAY" };

            try
            {
                JObject res = HttpGetRequest(url, 2000);

                if (res["status"].ToString() == "success")
                {
                    foreach (JToken reservations in res["data"]["reservations"])
                    {
                        if (probableStatus.Contains(reservations["stat"].ToString()))
                        {
                            res_id = reservations["id"].ToString();
                            if (alert)
                            {
                                Reservation reservation = new Reservation();
                                reservation.label2.Text = " ID: " + reservations["id"] + "\r\n 时间: " + reservations["date"] + " " + reservations["begin"] + "~" + reservations["end"];
                                status = reservations["stat"].ToString();
                                switch (status)
                                {
                                    case "RESERVE":
                                        reservation.label2.Text += "\r\n 状态: 预约";
                                        reservation.label3.Text = "是否取消此预约？（若不取消可自动改签座位）";
                                        reserving = true;
                                        break;
                                    case "CHECK_IN":
                                        if (reservations["awayEnd"].ToString() != "")
                                        {
                                            reservation.label2.Text += "\r\n 暂离时间: " + reservations["awayBegin"].ToString() + "~" + reservations["awayEnd"].ToString();
                                            reservation.label3.Location = new Point(120, 253);
                                        }
                                        reservation.label2.Text += "\r\n 状态: 履约中";
                                        reservation.label3.Text = "是否释放此座位？（若不释放可自动改签座位）";
                                        reserving = false;
                                        break;
                                    case "AWAY":
                                        reservation.label2.Text += "\r\n 暂离时间: " + reservations["awayBegin"].ToString();
                                        reservation.label2.Text += "\r\n 状态: 暂离";
                                        reservation.label3.Text = "是否释放此座位？（若不释放可自动改签座位）";
                                        reservation.label3.Location = new Point(120, 253);
                                        reserving = false;
                                        break;
                                }
                                reservation.label2.Text = reservation.label2.Text + "\r\n 地址: " + reservations["loc"].ToString() + "\r\n----------------------------------------------------------------";
                                reservation.Show();
                            }
                            else
                            {
                                status = reservations["stat"].ToString();
                                historyDate = reservations["date"].ToString();
                                historyStartTime = reservations["begin"].ToString();
                                historyEndTime = reservations["end"].ToString();
                                reserving = (status == "RESERVE") ? true : false;
                                if (status == "AWAY")
                                {
                                    historyAwayStartTime = reservations["awayBegin"].ToString();
                                }
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

        public static bool GetUsrInf(bool alert = true)
        {
            string url = API_V2_ROOT + "user";

            if (alert)
            {
                Config.config.textBox2.AppendText("\r\nFetching user information.....Status : ");
            }

            try
            {
                JObject res = HttpGetRequest(url);

                if (alert)
                {
                    Config.config.textBox2.AppendText(res["status"].ToString());
                }

                if (res["status"].ToString() == "success")
                {
                    name = res["data"]["name"].ToString();
                    last_login_time = res["data"]["lastLogin"].ToString();
                    if (res["data"]["checkedIn"].ToString() == "True")
                    {
                        checkedIn = true;
                        state = "已进入" + res["data"]["lastInBuildingName"].ToString();
                    }
                    else
                    {
                        checkedIn = false;
                        state = "未入馆";
                    }

                    violationCount = res["data"]["violationCount"].ToString();

                    return true;
                }
                else
                {
                    if (alert)
                    {
                        Config.config.textBox2.AppendText("\r\n" + res.ToString());
                    }
                    return false;
                }
            }
            catch
            {
                if (alert)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return false;
            }
        }

        public static bool GetRooms(string buildingId)
        {
            string url = API_V2_ROOT + "room/stats2/" + buildingId;
            Config.config.textBox2.AppendText("\r\nFetching room information.....Status : ");

            try
            {
                JObject res = HttpGetRequest(url);
                Config.config.textBox2.AppendText(res["status"].ToString());

                if (res["status"].ToString() == "success")
                {
                    JToken jToken = res["data"];
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
            string url = API_V2_ROOT + "room/layoutByDate/" + roomId + "/" + DateTime.Now.ToString("yyyy-MM-dd");
            Config.config.textBox2.AppendText("\r\nFetching seat information in room " + roomId + ".....Status : ");

            try
            {
                JObject res = HttpGetRequest(url);
                Config.config.textBox2.AppendText(res["status"].ToString());

                if (res["status"].ToString() == "success")
                {
                    JToken layout = res["data"]["layout"];
                    foreach (var num in layout)
                    {
                        if (num.First["type"].ToString() == "seat")
                        {
                            string seatInfo = num.First["name"].ToString();
                            if (num.First["power"].ToString() == "True")
                                seatInfo += " (电源)";
                            if (num.First["window"].ToString() == "True")
                                seatInfo += " (靠窗)";
                            if (num.First["computer"].ToString() == "True")
                                seatInfo += " (电脑)";
                            seats.Add(new DictionaryEntry(num.First["id"].ToString(), seatInfo));
                        }
                    }
                    NewComparer newComparer = new NewComparer();
                    seats.Sort(newComparer);

                    return true;
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n" + res.ToString());
                    return false;
                }
            }
            catch
            {
                Config.config.textBox2.AppendText("Connection lost");
                return false;
            }
        }

        public static bool CancelReservation(string id, bool alert = true)
        {
            string url = API_V2_ROOT + "cancel/" + id;

            if (alert)
            {
                Config.config.textBox2.AppendText("\r\nCancelling reservation.....Status : ");
            }

            try
            {
                JObject res = HttpGetRequest(url);

                if (alert)
                {
                    Config.config.textBox2.AppendText(res["status"].ToString());
                }

                if (res["status"].ToString() == "success")
                {
                    return true;
                }
                else
                {
                    if (alert)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，原因：" + res["message"].ToString());
                    }
                    return false;
                }
            }
            catch
            {
                if (alert)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return false;
            }
        }

        public static bool StopUsing(bool alert = true)
        {
            string url = API_V2_ROOT + "stop";

            if (alert)
            {
                Config.config.textBox2.AppendText("\r\nReleasing seat.....Status : ");
            }

            try
            {
                JObject res = HttpGetRequest(url);

                if (alert)
                {
                    Config.config.textBox2.AppendText(res["status"].ToString());
                }

                if (res["status"].ToString() == "success")
                {
                    return true;
                }
                else
                {
                    if (alert)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，原因：" + res["message"].ToString());
                    }
                    return false;
                }
            }
            catch
            {
                if (alert)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return false;
            }
        }

        public static string SearchFreeSeat(string buildingId, string roomId, string date, string startTime, string endTime)
        {
            if (startTime == "-1")
            {
                startTime = ((int)DateTime.Now.TimeOfDay.TotalMinutes).ToString();
            }

            string url = API_V2_ROOT + "searchSeats/" + date + "/" + startTime + "/" + endTime;
            Config.config.textBox2.AppendText("\r\nFetching free seats in room " + roomId + ".....Status : ");

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("{0}={1}", "t", "1");
            buffer.AppendFormat("&{0}={1}", "roomId", roomId);
            buffer.AppendFormat("&{0}={1}", "buildingId", buildingId);
            buffer.AppendFormat("&{0}={1}", "batch", "9999");
            buffer.AppendFormat("&{0}={1}", "page", "1");
            buffer.AppendFormat("&{0}={1}", "t2", "2");
            byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());

            try
            {
                JObject res = HttpPostRequest(url, data);

                if (res["data"]["seats"].ToString() != "{}")
                {
                    JToken seats = res["data"]["seats"];
                    foreach (var num in seats)
                    {
                        if (onlyPower && num.First["power"].ToString() == "False")
                        {
                            continue;
                        }
                        if (onlyWindow && num.First["window"].ToString() == "False")
                        {
                            continue;
                        }
                        if (onlyComputer && num.First["computer"].ToString() == "False")
                        {
                            continue;
                        }
                        freeSeats.Add(num.First["id"].ToString());
                    }
                    if (freeSeats.Count > 0)
                    {
                        Config.config.textBox2.AppendText("success");
                        return "Success";
                    }
                    else
                    {
                        Config.config.textBox2.AppendText("fail");
                        return "Failed";
                    }
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

        public static bool CheckStartTime(string seatId, string date, string startTime)
        {
            if (startTime == "-1")
            {
                startTime = "now";
            }

            string url = API_V2_ROOT + "startTimesForSeat/" + seatId + "/" + date;
            Config.config.textBox2.AppendText("\r\nChecking start time of seat No." + seatId + ".....Status : ");

            try
            {
                JObject res = HttpGetRequest(url);

                if (res["status"].ToString() == "success")
                {
                    startTimes.Clear();
                    JToken getStartTimes = res["data"]["startTimes"];
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
            if (startTime == "-1")
            {
                startTime = ((int)DateTime.Now.TimeOfDay.TotalMinutes).ToString();
            }

            string url = API_V2_ROOT + "endTimesForSeat/" + seatId + "/" + date + "/" + startTime;
            Config.config.textBox2.AppendText("\r\nChecking end time of seat No." + seatId + ".....Status : ");

            try
            {
                JObject res = HttpGetRequest(url);

                if (res["status"].ToString() == "success")
                {
                    endTimes.Clear();
                    JToken getEndTimes = res["data"]["endTimes"];
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

        public static string BookSeat(string seatId, string date, string startTime, string endTime, bool alert = true)
        {
            string url = API_V2_ROOT + "freeBook";

            StringBuilder buffer = new StringBuilder();
            buffer.AppendFormat("{0}={1}", "t", "1");
            buffer.AppendFormat("&{0}={1}", "startTime", startTime);
            buffer.AppendFormat("&{0}={1}", "endTime", endTime);
            buffer.AppendFormat("&{0}={1}", "seat", seatId);
            buffer.AppendFormat("&{0}={1}", "date", date);
            buffer.AppendFormat("&{0}={1}", "t2", "2");
            byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());

            if (alert)
            {
                Config.config.textBox2.AppendText("\r\n\r\nBooking seat No." + seatId + ".....Status : ");
            }

            try
            {
                JObject res = HttpPostRequest(url, data);

                if (alert)
                {
                    Config.config.textBox2.AppendText(res["status"].ToString() + "\r\n");
                }

                if (res["status"].ToString() == "success")
                {
                    bookedSeatId = seatId;
                    if (alert)
                    {
                        PrintBookInf(res);
                        if (Config.config.checkBox2.Checked)
                        {
                            res.Add("username", username);
                            res.Add("name", name);
                            res.Add("client", "C#");
                            res.Add("version", Application.ProductVersion);
                            SendMail(res.ToString(), to_addr);
                        }
                    }
                    return "Success";
                }
                else
                {
                    if (alert)
                    {
                        Config.config.textBox2.AppendText("\r\n预约座位失败，原因：" + res["message"].ToString());
                    }
                    return "Failed";
                }
            }
            catch
            {
                if (alert)
                {
                    Config.config.textBox2.AppendText("Connection lost");
                }
                return "Connection lost";
            }
        }

        public static void PrintBookInf(JObject res)
        {
            Config.config.textBox2.AppendText("\r\n---------------------------座位预约成功---------------------------");
            Config.config.textBox2.AppendText("\r\nID：" + res["data"]["id"]);
            Config.config.textBox2.AppendText("\r\n凭证号码：" + res["data"]["receipt"]);
            Config.config.textBox2.AppendText("\r\n时间：" + res["data"]["onDate"] + " " + res["data"]["begin"] + " ~ " + res["data"]["end"]);
            if (res["data"]["checkedIn"].ToString() == "False")
                Config.config.textBox2.AppendText("\r\n状态：预约");
            else
                Config.config.textBox2.AppendText("\r\n状态：已签到");
            Config.config.textBox2.AppendText("\r\n地址：" + res["data"]["location"]);
            Config.config.textBox2.AppendText("\r\n--------------------------------------------------------------------");
        }

        public static void SendMail(string json, string to_addr)
        {
            if (Regex.IsMatch(to_addr, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
            {
                Config.config.textBox2.AppendText("\r\n\r\n尝试连接邮件服务器");
                byte[] data = new byte[5];
                Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(SERVER);
                IPEndPoint point = new IPEndPoint(ip, 5210);

                try
                {
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
                        socketClient.Send(Encoding.UTF8.GetBytes("to" + to_addr));
                        socketClient.Receive(data);
                        Config.config.textBox2.AppendText("\r\n" + Encoding.UTF8.GetString(data));

                        Config.config.textBox2.AppendText("\r\n正在发送邮件提醒至\"" + to_addr + "\"");
                        data = new byte[7];
                        socketClient.Send(Encoding.UTF8.GetBytes("SendMail"));
                        socketClient.Receive(data);
                        stringData = Encoding.UTF8.GetString(data);
                        if (stringData == "success")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n邮件提醒发送成功\r\n若接收不到提醒，请将\"seatkiller@outlook.com\"添加至邮箱白名单");
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n邮箱提醒发送失败");
                        }
                        socketClient.Send(Encoding.UTF8.GetBytes("exit"));
                        socketClient.Close();
                    }
                }
                catch
                {
                    Config.config.textBox2.AppendText("\r\n连接失败，邮件发送取消");
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

        public static void LoggedIn()
        {
            byte[] data = new byte[5];
            Socket socketClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(SERVER);
            IPEndPoint point = new IPEndPoint(ip, 5210);

            try
            {
                socketClient.Connect(point);
                socketClient.Receive(data);
                string stringData = Encoding.UTF8.GetString(data);

                if (stringData == "Hello")
                {

                    data = new byte[128];
                    socketClient.Send(Encoding.UTF8.GetBytes("login" + username + name + " (" + Application.ProductVersion + ")"));
                    Thread.Sleep(1000);

                    socketClient.Send(Encoding.UTF8.GetBytes("exit"));
                    socketClient.Close();
                }
            }
            catch { }
        }

        public static void LockSeat(string seatId)
        {
            int index, linesCount, count = 0;
            bool doClear = false, reBook = false;
            Config.config.textBox2.AppendText("\r\n正在锁定座位，ID: " + seatId + "\r\n");
            if (!CheckResInf(false))
            {
                Config.config.textBox2.AppendText("\r\n\r\n预约信息获取失败");
                return;
            }
            while (true)
            {
                if (count >= 50)
                {
                    Config.config.textBox2.AppendText("\r\n\r\n座位锁定失败");
                    break;
                }

                if (historyDate == DateTime.Now.ToString("yyyy-M-d") & DateTime.Now.TimeOfDay.TotalMinutes > 400 & DateTime.Now.TimeOfDay.TotalMinutes < 1320)
                {
                    if (GetToken(false) == "Success")
                    {
                        if (CheckResInf(false) || reBook)
                        {
                            int historyEndTimeInt = int.Parse(historyEndTime.Substring(0, 2)) * 60 + int.Parse(historyEndTime.Substring(3, 2));

                            if (historyEndTimeInt - (int)DateTime.Now.TimeOfDay.TotalMinutes < 2)
                            {
                                if (reserving)
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n座位预约时间已过，自动取消预约");
                                    CancelReservation(res_id);
                                }
                                else
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n座位预约时间已过，自动释放座位");
                                    StopUsing();
                                }
                                break;
                            }

                            if (reserving & !checkedIn)
                            {
                                int historyStartTimeInt = int.Parse(historyStartTime.Substring(0, 2)) * 60 + int.Parse(historyStartTime.Substring(3, 2));

                                if ((int)DateTime.Now.TimeOfDay.TotalMinutes - historyStartTimeInt >= 25)
                                {
                                    if (CancelReservation(res_id, false) || reBook)
                                    {
                                        if (BookSeat(seatId, DateTime.Now.ToString("yyyy-MM-dd"), "-1", historyEndTimeInt.ToString(), false) != "Success")
                                        {
                                            if (doClear)
                                            {
                                                index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                                                Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                                                Config.config.textBox2.SelectedText = "重新预约座位失败，重试次数: " + count;
                                            }
                                            else
                                            {
                                                Config.config.textBox2.AppendText("\r\n重新预约座位失败，重试次数: " + count);
                                                doClear = true;
                                            }
                                            Thread.Sleep(5000);
                                            reBook = true;
                                            count += 1;
                                            continue;
                                        }
                                        else
                                        {
                                            reBook = false;
                                        }
                                    }
                                    else
                                    {
                                        if (doClear)
                                        {
                                            index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                                            Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                                            Config.config.textBox2.SelectedText = "取消预约失败，重试次数: " + count;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n取消预约失败，重试次数: " + count);
                                            doClear = true;
                                        }
                                        Thread.Sleep(5000);
                                        count += 1;
                                        continue;
                                    }
                                }
                            }
                            else if (status == "AWAY")
                            {
                                int historyAwayStartTimeInt = int.Parse(historyAwayStartTime.Substring(0, 2)) * 60 + int.Parse(historyAwayStartTime.Substring(3, 2));

                                if ((int)DateTime.Now.TimeOfDay.TotalMinutes - historyAwayStartTimeInt >= 25)
                                {
                                    if (StopUsing(false) || reBook)
                                    {
                                        if (BookSeat(seatId, DateTime.Now.ToString("yyyy-MM-dd"), "-1", historyEndTimeInt.ToString(), false) != "Success")
                                        {
                                            if (doClear)
                                            {
                                                index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                                                Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                                                Config.config.textBox2.SelectedText = "重新预约座位失败，重试次数: " + count;
                                            }
                                            else
                                            {
                                                Config.config.textBox2.AppendText("\r\n重新预约座位失败，重试次数: " + count);
                                                doClear = true;
                                            }
                                            Thread.Sleep(5000);
                                            reBook = true;
                                            count += 1;
                                            continue;
                                        }
                                        else
                                        {
                                            reBook = false;
                                        }
                                    }
                                    else
                                    {
                                        if (doClear)
                                        {
                                            index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                                            Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                                            Config.config.textBox2.SelectedText = "释放座位失败，重试次数: " + count;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n释放座位失败，重试次数: " + count);
                                            doClear = true;
                                        }
                                        Thread.Sleep(5000);
                                        count += 1;
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (doClear)
                            {
                                index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                                Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                                Config.config.textBox2.SelectedText = "获取预约信息失败，重试次数: " + count;
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n获取预约信息失败，重试次数: " + count);
                                doClear = true;
                            }
                            Thread.Sleep(5000);
                            count += 1;
                            continue;
                        }
                    }
                    else
                    {
                        if (doClear)
                        {
                            index = Config.config.textBox2.GetFirstCharIndexOfCurrentLine();
                            Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                            Config.config.textBox2.SelectedText = "获取token失败，重试次数: " + count;
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n获取token失败，重试次数: " + count);
                            doClear = true;
                        }
                        Thread.Sleep(5000);
                        count += 1;
                        continue;
                    }
                }
                else if (historyDate == DateTime.Now.ToString("yyyy-M-d") & DateTime.Now.TimeOfDay.TotalMinutes > 1320)
                {
                    return;
                }
                count = 0;
                linesCount = Config.config.textBox2.Lines.Count();
                index = Config.config.textBox2.GetFirstCharIndexFromLine(linesCount - (doClear ? 2 : 1));
                Config.config.textBox2.Select(index, Config.config.textBox2.TextLength - index);
                Config.config.textBox2.SelectedText = "当前有效" + (reserving ? "预约" : "使用") + "时间: " + historyDate + " " + historyStartTime + "~" + historyEndTime;
                doClear = false;
                Thread.Sleep(30000);
            }
        }

        public static bool Loop(string buildingId, string[] rooms, string startTime, string endTime, string roomId = "0", string seatId = "0")
        {
            Config.config.textBox2.AppendText("\r\n\r\n---------------------------进入捡漏模式---------------------------\r\n");
            string date = DateTime.Now.ToString("yyyy-MM-dd");

            if (DateTime.Now.TimeOfDay.TotalMinutes < 60 || DateTime.Now.TimeOfDay.TotalMinutes > 1420)
            {
                Wait("01", "00", "00", false);
            }
            else if (DateTime.Now.TimeOfDay.TotalMinutes > 1320)
            {
                Config.config.textBox2.AppendText("\r\n捡漏失败，超出运行时间\r\n");
                Config.config.textBox2.AppendText("\r\n---------------------------退出捡漏模式---------------------------\r\n");
                return false;
            }

            GetRooms(buildingId);

            if (seatId != "0" && !Config.config.checkBox1.Checked)
            {
                Config.config.textBox2.AppendText("\r\n正在监控座位，ID: " + seatId + "\r\n");
            }
            else if (roomId != "0" && !Config.config.checkBox1.Checked)
            {
                Config.config.textBox2.AppendText("\r\n正在监控区域，ID: " + roomId + "\r\n");
            }

            while (true)
            {
                if (DateTime.Now.TimeOfDay.TotalMinutes > 1320)
                {
                    Config.config.textBox2.AppendText("\r\n\r\n捡漏失败，超出运行时间\r\n");
                    Config.config.textBox2.AppendText("\r\n---------------------------退出捡漏模式---------------------------\r\n");
                    return false;
                }

                if (seatId != "0")
                {
                    string res = BookSeat(seatId, date, startTime, endTime);

                    if (res == "Success")
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                        Config.config.textBox2.AppendText("\r\n---------------------------退出捡漏模式---------------------------\r\n");
                        return true;
                    }
                    else if (res == "Connection lost")
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续预约空位\r\n");
                        Thread.Sleep(30000);
                        continue;
                    }
                    else if (Config.config.checkBox1.Checked)
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n指定座位预约失败，尝试检索其他空位.....\r\n");
                        seatId = "0";
                        continue;
                    }
                }
                else
                {
                    freeSeats.Clear();

                    if (roomId == "0")
                    {
                        foreach (var room in rooms)
                        {
                            if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                Thread.Sleep(30000);
                            }
                        }
                    }
                    else
                    {
                        string res = SearchFreeSeat(buildingId, roomId, date, startTime, endTime);
                        if (res == "Connection lost")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                            Thread.Sleep(30000);
                        }
                        else if (res == "Failed" && Config.config.checkBox1.Checked)
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n当前区域暂无空位，尝试全馆检索空位.....\r\n");
                            foreach (var room in rooms)
                            {
                                if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                    Thread.Sleep(30000);
                                }
                            }
                        }
                    }

                    foreach (var freeSeatId in freeSeats)
                    {
                        switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                        {
                            case "Success":
                                Config.config.textBox2.AppendText("\r\n\r\n捡漏成功\r\n");
                                Config.config.textBox2.AppendText("\r\n---------------------------退出捡漏模式---------------------------\r\n");
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

                Config.config.textBox2.AppendText("\r\n\r\n循环结束，10秒后进入下一个循环，运行时间剩余" + (79200 - (int)DateTime.Now.TimeOfDay.TotalSeconds).ToString() + "秒\r\n");
                Thread.Sleep(10000);
            }
        }

        public static bool ExchangeLoop(string buildingId, string[] rooms, string startTime, string endTime, string roomId = "0", string seatId = "0")
        {
            Config.config.textBox2.AppendText("\r\n\r\n---------------------------进入改签模式---------------------------\r\n");
            bool cancelled = false;
            string date = DateTime.Now.ToString("yyyy-MM-dd");

            if (DateTime.Now.TimeOfDay.TotalMinutes < 60 || DateTime.Now.TimeOfDay.TotalMinutes > 1420)
            {
                Wait("01", "00", "00", false);
            }
            else if (DateTime.Now.TimeOfDay.TotalMinutes > 1320)
            {
                Config.config.textBox2.AppendText("\r\n改签失败，超出运行时间\r\n");
                Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                return false;
            }

            GetRooms(buildingId);

            if (seatId != "0" && !Config.config.checkBox1.Checked)
            {
                Config.config.textBox2.AppendText("\r\n正在监控座位，ID: " + seatId + "\r\n");
            }
            else if (roomId != "0" && !Config.config.checkBox1.Checked)
            {
                Config.config.textBox2.AppendText("\r\n正在监控区域，ID: " + roomId + "\r\n");
            }

            while (true)
            {
                if (DateTime.Now.TimeOfDay.TotalMinutes > 1320)
                {
                    Config.config.textBox2.AppendText("\r\n\r\n改签失败，超出运行时间\r\n");
                    Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                    return false;
                }

                if (seatId != "0")
                {
                    if (CheckStartTime(seatId, date, startTime) && CheckEndTime(seatId, date, startTime, endTime))
                    {
                        GetUsrInf(true);
                        if (!reserving)
                        {
                            if (StopUsing())
                            {
                                if (BookSeat(seatId, date, startTime, endTime) == "Success")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                                    return true;
                                }
                                else
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n改签失败，原座位已丢失\r\n");
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                                    return false;
                                }
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出改签模式---------------------------\r\n");
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
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                                    return true;
                                }
                                else
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n改签失败，原座位已丢失\r\n");
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
                                    return false;
                                }
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出改签模式---------------------------\r\n");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    freeSeats.Clear();

                    if (roomId == "0")
                    {
                        foreach (var room in rooms)
                        {
                            if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                Thread.Sleep(30000);
                            }
                        }
                    }
                    else
                    {
                        string res = SearchFreeSeat(buildingId, roomId, date, startTime, endTime);
                        if (res == "Connection lost")
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                            Thread.Sleep(30000);
                        }
                        else if (res == "Failed" && Config.config.checkBox1.Checked)
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n当前区域暂无空位，尝试全馆检索空位.....\r\n");
                            foreach (var room in rooms)
                            {
                                if (SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                    Thread.Sleep(30000);
                                }
                            }
                        }
                    }

                    foreach (var freeSeatId in freeSeats)
                    {
                        if (!cancelled)
                        {
                            if (CheckStartTime(freeSeatId.ToString(), date, startTime) && CheckEndTime(freeSeatId.ToString(), date, startTime, endTime))
                            {
                                GetUsrInf(true);
                                if (!reserving)
                                {
                                    if (StopUsing())
                                    {
                                        cancelled = true;
                                    }
                                    else
                                    {
                                        Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出改签模式---------------------------\r\n");
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
                                        Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出改签模式---------------------------\r\n");
                                        return false;
                                    }
                                }

                                switch (BookSeat(freeSeatId.ToString(), date, startTime, endTime))
                                {
                                    case "Success":
                                        Config.config.textBox2.AppendText("\r\n\r\n改签成功\r\n");
                                        Config.config.textBox2.AppendText("\r\n---------------------------退出改签模式---------------------------\r\n");
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
                    }
                }

                Config.config.textBox2.AppendText("\r\n\r\n循环结束，10秒后进入下一个循环，运行时间剩余" + (79200 - (int)DateTime.Now.TimeOfDay.TotalSeconds).ToString() + "秒\r\n");
                Thread.Sleep(10000);
            }
        }
    }
}