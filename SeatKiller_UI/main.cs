using System;
using System.Threading;

namespace SeatKiller_UI
{
    public static class main
    {
        public static string buildingId, roomId, seatId, date, startTime, endTime;
        public static string[] rooms;
        private static Thread thread;

        public static void Start()
        {
            thread = new Thread(run);
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Stop()
        {
            bool waiting = false;
            try
            {
                if (Config.config.backgroundWorker1.IsBusy)
                {
                    waiting = true;
                }
                Config.config.backgroundWorker1.CancelAsync();
                while (true)
                {
                    if (!Config.config.backgroundWorker1.IsBusy)
                    {
                        break;
                    }
                }
                thread.Abort();
                if (waiting)
                {
                    Config.config.textBox2.AppendText("\r\n-----------------------------运行中断------------------------------\r\n");
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n\r\n-----------------------------运行中断------------------------------\r\n");
                }

            }
            catch
            {
                if (waiting)
                {
                    Config.config.textBox2.AppendText("\r\n-----------------------------运行中断------------------------------\r\n");
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n\r\n-----------------------------运行中断------------------------------\r\n");
                }
            }
        }

        public static void run()
        {
            bool cancelled = false;
            if (Config.config.comboBox3.SelectedIndex == 1)
            {
                Config.config.textBox2.AppendText("\r\n\r\n---------------------------进入抢座模式---------------------------\r\n");

                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 22:14:40")) < 0)
                    SeatKiller.Wait("22", "14", "40", false);
                bool try_booking = true;

                if (SeatKiller.GetToken() == "Success")
                {
                    SeatKiller.GetBuildings();
                    SeatKiller.GetRooms(buildingId);
                    if (SeatKiller.CheckResInf(false))
                    {
                        Config.config.textBox2.AppendText("\r\n已检测到有效预约，将自动改签预约信息\r\n");
                        SeatKiller.exchange = true;
                    }

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 22:15:00")) < 0)
                        SeatKiller.Wait("22", "15", "00");
                    else if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n预约系统开放时间已过，准备进入捡漏模式");
                        SeatKiller.Wait("01", "00", "00");

                        if (SeatKiller.exchange)
                            SeatKiller.ExchangeLoop(buildingId, rooms, startTime, endTime, roomId, seatId);
                        else
                            SeatKiller.Loop(buildingId, rooms, startTime, endTime, roomId, seatId);

                        EnableControls();
                        return;
                    }

                    while (try_booking)
                    {
                        if (seatId != "0")
                        {
                            if (SeatKiller.exchange & SeatKiller.check_in & !cancelled)
                            {
                                if (SeatKiller.StopUsing())
                                {
                                    cancelled = true;
                                }
                                else
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，请稍后重试\r\n");
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                                    EnableControls();
                                    return;
                                }
                            }
                            else if (SeatKiller.exchange & !SeatKiller.check_in & !cancelled)
                            {
                                if (SeatKiller.CancelReservation(SeatKiller.res_id))
                                {
                                    cancelled = true;
                                }
                                else
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，请稍后重试\r\n");
                                    Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                                    EnableControls();
                                    return;
                                }
                            }

                            if (SeatKiller.BookSeat(seatId, date, startTime, endTime) == "Success")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出抢座模式---------------------------\r\n");
                                EnableControls();
                                return;
                            }
                            else if (Config.config.checkBox1.Checked)
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n指定座位预约失败，尝试检索其他空位.....");
                                seatId = "0";
                                continue;
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n抢座失败\r\n");
                                Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                                EnableControls();
                                return;
                            }
                        }
                        else if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00")) < 0)
                        {
                            SeatKiller.freeSeats.Clear();
                            if (roomId == "0")
                            {
                                foreach (var room in rooms)
                                {
                                    if (SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                    {
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                        Thread.Sleep(30000);
                                    }
                                }
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n尝试检索同区域其他座位.....\r\n");
                                if (SeatKiller.SearchFreeSeat(buildingId, roomId, date, startTime, endTime) != "Success")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n当前区域暂无空位，尝试全馆检索空位.....\r\n");
                                    foreach (var room in rooms)
                                    {
                                        if (SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime) == "Connection lost")
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                            Thread.Sleep(30000);
                                        }
                                    }
                                }
                            }

                            if (SeatKiller.freeSeats.Count > 0)
                            {
                                foreach (var freeSeat in SeatKiller.freeSeats)
                                {
                                    if (SeatKiller.exchange & SeatKiller.check_in & !cancelled)
                                    {
                                        if (SeatKiller.StopUsing())
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n释放座位失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                                            EnableControls();
                                            return;
                                        }
                                    }
                                    else if (SeatKiller.exchange & !SeatKiller.check_in & !cancelled)
                                    {
                                        if (SeatKiller.CancelReservation(SeatKiller.res_id))
                                        {
                                            cancelled = true;
                                        }
                                        else
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n取消预约失败，请稍后重试\r\n");
                                            Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                                            EnableControls();
                                            return;
                                        }
                                    }

                                    switch (SeatKiller.BookSeat(freeSeat.ToString(), date, startTime, endTime))
                                    {
                                        case "Success":
                                            Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出抢座模式---------------------------\r\n");
                                            EnableControls();
                                            return;
                                        case "Failed":
                                            Thread.Sleep(2000);
                                            break;
                                        case "Connection lost":
                                            DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00");
                                            TimeSpan delta = time.Subtract(DateTime.Now);
                                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，1分钟后重新尝试抢座，系统开放时间剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n");
                                            Thread.Sleep(60000);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n当前全馆暂无空位，5秒后尝试继续检索空位\r\n");
                                Thread.Sleep(5000);
                            }
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n抢座失败，座位预约系统已关闭\r\n");
                            Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                            EnableControls();
                            return;
                        }
                    }
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n\r\n获取token失败，请检查网络后重试\r\n");
                    Config.config.textBox2.AppendText("\r\n---------------------------退出抢座模式---------------------------\r\n");
                    EnableControls();
                    return;
                }
            }
            else
            {
                if (SeatKiller.CheckResInf(false))
                {
                    Config.config.textBox2.AppendText("\r\n\r\n已检测到有效预约，将自动改签预约信息");
                    SeatKiller.exchange = true;
                }

                if (SeatKiller.exchange)
                {
                    SeatKiller.ExchangeLoop(buildingId, rooms, startTime, endTime, roomId, seatId);
                }
                else
                {
                    SeatKiller.Loop(buildingId, rooms, startTime, endTime, roomId, seatId);
                }

                EnableControls();
                return;
            }
        }

        public static void EnableControls()
        {
            Config.config.label2.Enabled = true;
            Config.config.label3.Enabled = true;
            Config.config.label4.Enabled = true;
            Config.config.label5.Enabled = true;
            Config.config.label6.Enabled = true;
            Config.config.label8.Enabled = true;
            Config.config.label9.Enabled = true;
            Config.config.comboBox1.Enabled = true;
            Config.config.comboBox2.Enabled = true;
            Config.config.comboBox3.Enabled = true;
            Config.config.comboBox4.Enabled = true;
            Config.config.comboBox5.Enabled = true;
            Config.config.comboBox6.Enabled = true;
            Config.config.checkBox1.Enabled = true;
            Config.config.checkBox2.Enabled = true;
            Config.config.checkBox3.Enabled = true;
            Config.config.checkBox4.Enabled = true;
            Config.config.checkBox5.Enabled = true;
            if (Config.config.checkBox2.Checked)
            {
                Config.config.label7.Enabled = true;
                Config.config.textBox1.Enabled = true;
            }
            Config.config.button1.Text = "开始抢座";
        }
    }
}
