﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            try
            {
                thread.Abort();
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------抢座模式中断------------------------------\r\n");
            }
            catch
            {
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------抢座模式中断------------------------------\r\n");
            }
        }

        public static void run()
        {
            if (Config.config.comboBox3.SelectedIndex == 1)
            {
                Config.config.textBox2.AppendText("\r\n\r\n------------------------------进入抢座模式------------------------------\r\n");
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 22:14:40")) < 0)
                    SeatKiller.Wait("22", "14", "40");
                bool try_booking = true;
                if (SeatKiller.GetToken() == "Success")
                {
                    SeatKiller.GetBuildings();
                    SeatKiller.GetRooms(buildingId);

                    if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 22:15:00")) < 0)
                        SeatKiller.Wait("22", "15", "00");
                    else if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00")) > 0)
                    {
                        Config.config.textBox2.AppendText("\r\n预约系统开放时间已过，准备进入捡漏模式");
                        SeatKiller.Wait("01", "00", "00");
                        SeatKiller.Loop(buildingId, rooms, startTime, endTime);
                        return;
                    }
                    while (try_booking)
                    {
                        if (seatId != "0")
                        {
                            if (SeatKiller.BookSeat(seatId, date, startTime, endTime) == "Success")
                                return;
                            else if (Config.config.checkBox1.Checked)
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n指定座位预约失败，尝试检索其他空位.....");
                                seatId = "0";
                                continue;
                            }
                            else
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n抢座失败\r\n");
                                Config.config.textBox2.AppendText("\r\n------------------------------退出抢座模式------------------------------\r\n");
                                Config.config.comboBox1.Enabled = true;
                                Config.config.comboBox2.Enabled = true;
                                Config.config.comboBox3.Enabled = true;
                                Config.config.comboBox4.Enabled = true;
                                Config.config.comboBox5.Enabled = true;
                                Config.config.comboBox6.Enabled = true;
                                Config.config.checkBox1.Enabled = true;
                                Config.config.checkBox2.Enabled = true;
                                Config.config.textBox1.Enabled = true;
                                Config.config.button1.Text = "开始抢座";
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

                            if (SeatKiller.freeSeats.Count == 0)
                            {
                                Config.config.textBox2.AppendText("\r\n\r\n当前全馆暂无空位，5秒后尝试继续检索空位\r\n");
                                Thread.Sleep(5000);
                            }

                            foreach (var freeSeat in SeatKiller.freeSeats)
                            {
                                string response = SeatKiller.BookSeat(freeSeat.ToString(), date, startTime, endTime);
                                switch (response)
                                {
                                    case "Success":
                                        try_booking = false;
                                        return;
                                    case "Failed":
                                        Thread.Sleep(2000);
                                        break;
                                    case "Connection lost":
                                        DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:45:00");
                                        TimeSpan delta = time.Subtract(DateTime.Now);
                                        Config.config.textBox2.AppendText("\r\n\r\n连接丢失，1分钟后重新尝试抢座，系统开放时间剩余" + delta.TotalSeconds.ToString() + "秒\r\n");
                                        Thread.Sleep(60000);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Config.config.textBox2.AppendText("\r\n\r\n抢座失败，座位预约系统已关闭\r\n");
                            break;
                        }
                    }
                }
                else
                {
                    Config.config.textBox2.AppendText("\r\n\r\n获取token失败，请检查网络后重试\r\n");
                    Config.config.textBox2.AppendText("\r\n------------------------------退出抢座模式------------------------------\r\n");
                    Config.config.comboBox1.Enabled = true;
                    Config.config.comboBox2.Enabled = true;
                    Config.config.comboBox3.Enabled = true;
                    Config.config.comboBox4.Enabled = true;
                    Config.config.comboBox5.Enabled = true;
                    Config.config.comboBox6.Enabled = true;
                    Config.config.checkBox1.Enabled = true;
                    Config.config.checkBox2.Enabled = true;
                    Config.config.textBox1.Enabled = true;
                    Config.config.button1.Text = "开始抢座";
                }
            }
            else
            {
                SeatKiller.Loop(buildingId, rooms, startTime, endTime, seatId);
                Config.config.comboBox1.Enabled = true;
                Config.config.comboBox2.Enabled = true;
                Config.config.comboBox3.Enabled = true;
                Config.config.comboBox4.Enabled = true;
                Config.config.comboBox5.Enabled = true;
                Config.config.comboBox6.Enabled = true;
                Config.config.checkBox1.Enabled = true;
                Config.config.checkBox2.Enabled = true;
                Config.config.textBox1.Enabled = true;
                Config.config.button1.Text = "开始抢座";
                return;
            }
        }
    }
}
