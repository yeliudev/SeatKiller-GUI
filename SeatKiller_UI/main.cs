using System;
using System.Threading;

namespace SeatKiller_UI
{
    public static class Main
    {
        public static string buildingId, roomId, seatId, date, startTime, endTime;
        public static string[] rooms;
        public static bool enter;
        private static Thread thread;

        public static void Start(bool didClear = false)
        {
            enter = !didClear;
            if (Config.config.backgroundWorker4.IsBusy)
            {
                Config.config.backgroundWorker4.CancelAsync();
            }
            thread = new Thread(Run)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public static void Stop()
        {
            bool workerBusy = false;
            if (Config.config.backgroundWorker1.IsBusy)
            {
                workerBusy = true;
                Config.config.backgroundWorker1.CancelAsync();
            }
            if (Config.config.backgroundWorker4.IsBusy)
            {
                workerBusy = true;
                Config.config.backgroundWorker4.CancelAsync();
            }
            while (Config.config.backgroundWorker1.IsBusy || Config.config.backgroundWorker4.IsBusy)
            {
                Thread.Sleep(100);
            }
            thread.Abort();
            Config.config.textBox2.AppendText((workerBusy ? "" : "\r\n") + "\r\n-----------------------------运行中断------------------------------\r\n");
        }

        public static void Run()
        {
            bool cancelled = false, exchange = false;
            if (Config.config.comboBox3.SelectedIndex == 1)
            {
                Config.config.textBox2.AppendText((enter ? "\r\n" : "") + "\r\n---------------------------进入抢座模式---------------------------\r\n");

                if (DateTime.Now.TimeOfDay.TotalSeconds < 81880)
                {
                    SeatKiller.Wait("22", "44", "40", false);
                }

                if (SeatKiller.GetToken() == "Success")
                {
                    if (SeatKiller.CheckResInf(false))
                    {
                        Config.config.textBox2.AppendText("\r\n已检测到有效预约，将自动改签预约信息\r\n");
                        exchange = true;
                    }
                    else
                    {
                        exchange = false;
                    }

                    if (DateTime.Now.TimeOfDay.TotalMinutes < 1365)
                    {
                        SeatKiller.GetRooms(buildingId);
                        SeatKiller.Wait("22", "45", "00");
                    }
                    else if (DateTime.Now.TimeOfDay.TotalMinutes > 1420)
                    {
                        Config.config.textBox2.AppendText("\r\n预约系统已关闭");

                        if (exchange)
                        {
                            if (SeatKiller.ExchangeLoop(buildingId, rooms, startTime, endTime, roomId, seatId))
                            {
                                SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                if (Config.config.checkBox6.Checked)
                                {
                                    Config.config.backgroundWorker4.RunWorkerAsync();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (SeatKiller.Loop(buildingId, rooms, startTime, endTime, roomId, seatId))
                            {
                                SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                if (Config.config.checkBox6.Checked)
                                {
                                    Config.config.backgroundWorker4.RunWorkerAsync();
                                    return;
                                }
                            }
                        }

                        EnableControls();
                        return;
                    }

                    while (true)
                    {
                        if (DateTime.Now.TimeOfDay.TotalMinutes > 1420)
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n抢座失败，座位预约系统已关闭");

                            if (exchange)
                            {
                                if (SeatKiller.ExchangeLoop(buildingId, rooms, startTime, endTime, roomId, seatId))
                                {
                                    SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                }

                                if (Config.config.checkBox6.Checked)
                                {
                                    Config.config.backgroundWorker4.RunWorkerAsync();
                                }
                                else
                                {
                                    EnableControls();
                                }
                            }
                            else
                            {
                                if (SeatKiller.Loop(buildingId, rooms, startTime, endTime, roomId, seatId))
                                {
                                    SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                }

                                if (Config.config.checkBox6.Checked)
                                {
                                    Config.config.backgroundWorker4.RunWorkerAsync();
                                }
                                else
                                {
                                    EnableControls();
                                }
                            }

                            return;
                        }

                        if (seatId != "0")
                        {
                            if (exchange && !SeatKiller.reserving && !cancelled)
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
                            else if (exchange && SeatKiller.reserving && !cancelled)
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

                            string res = SeatKiller.BookSeat(seatId, date, startTime, endTime);
                            if (res == "Success")
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出抢座模式---------------------------\r\n");
                                SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                if (Config.config.checkBox6.Checked)
                                {
                                    Config.config.backgroundWorker4.RunWorkerAsync();
                                }
                                else
                                {
                                    EnableControls();
                                }
                                return;
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
                            SeatKiller.freeSeats.Clear();

                            if (roomId == "0")
                            {
                                foreach (var room in rooms)
                                {
                                    string res = SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime);
                                    if (res == "Success")
                                    {
                                        break;
                                    }
                                    else if (res == "Connection lost")
                                    {
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                        Thread.Sleep(30000);
                                        continue;
                                    }
                                    Thread.Sleep(1500);
                                }
                            }
                            else
                            {
                                string res = SeatKiller.SearchFreeSeat(buildingId, roomId, date, startTime, endTime);
                                if (res == "Connection lost")
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                    Thread.Sleep(30000);
                                    continue;
                                }
                                else if (res == "Failed" && Config.config.checkBox1.Checked)
                                {
                                    Config.config.textBox2.AppendText("\r\n\r\n当前区域暂无空位，尝试全馆检索空位.....\r\n");
                                    foreach (var room in rooms)
                                    {
                                        string result = SeatKiller.SearchFreeSeat(buildingId, room, date, startTime, endTime);
                                        if (result == "Success")
                                        {
                                            break;
                                        }
                                        else if (result == "Connection lost")
                                        {
                                            Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后尝试继续检索空位\r\n");
                                            Thread.Sleep(30000);
                                            continue;
                                        }
                                        Thread.Sleep(1500);
                                    }
                                }
                            }

                            foreach (var freeSeat in SeatKiller.freeSeats)
                            {
                                if (exchange && !SeatKiller.reserving && !cancelled)
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
                                else if (exchange && SeatKiller.reserving && !cancelled)
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
                                        SeatKiller.bookedSeatId = freeSeat.ToString();
                                        Config.config.textBox2.AppendText("\r\n\r\n---------------------------退出抢座模式---------------------------\r\n");
                                        SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                                        if (Config.config.checkBox6.Checked)
                                        {
                                            Config.config.backgroundWorker4.RunWorkerAsync();
                                        }
                                        else
                                        {
                                            EnableControls();
                                        }
                                        return;
                                    case "Failed":
                                        Thread.Sleep(1500);
                                        break;
                                    case "Connection lost":
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，30秒后重新尝试抢座，系统开放时间剩余" + (85500 - (int)DateTime.Now.TimeOfDay.TotalSeconds).ToString() + "秒\r\n");
                                        Thread.Sleep(30000);
                                        break;
                                }
                            }
                        }

                        Config.config.textBox2.AppendText("\r\n\r\n暂无可用座位，系统开放时间剩余" + (85200 - (int)DateTime.Now.TimeOfDay.TotalSeconds).ToString() + "秒\r\n");
                        Thread.Sleep(1500);
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
                if (SeatKiller.GetToken(false) == "Success")
                {
                    if (SeatKiller.CheckResInf(false))
                    {
                        Config.config.textBox2.AppendText("\r\n\r\n已检测到有效预约，将自动改签预约信息");
                        if (SeatKiller.ExchangeLoop(buildingId, rooms, startTime, endTime, roomId, seatId))
                        {
                            SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                        }

                        if (Config.config.checkBox6.Checked)
                        {
                            Config.config.backgroundWorker4.RunWorkerAsync();
                        }
                        else
                        {
                            EnableControls();
                        }
                    }
                    else
                    {
                        if (SeatKiller.Loop(buildingId, rooms, startTime, endTime, roomId, seatId))
                        {
                            SeatKiller.LockSeat(SeatKiller.bookedSeatId);
                        }

                        if (Config.config.checkBox6.Checked)
                        {
                            Config.config.backgroundWorker4.RunWorkerAsync();
                        }
                        else
                        {
                            EnableControls();
                        }
                    }
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n\r\n获取token失败，请检查网络后重试\r\n");
                    EnableControls();
                }

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
            Config.config.checkBox6.Enabled = true;
            Config.config.pictureBox2.Enabled = true;
            Config.config.pictureBox2.Image = Properties.Resources.description_active;
            if (Config.config.checkBox2.Checked)
            {
                Config.config.label7.Enabled = true;
                Config.config.textBox1.Enabled = true;
            }
            Config.config.button1.Text = "开始抢座";
        }
    }
}