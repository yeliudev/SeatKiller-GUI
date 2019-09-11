using System;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace SeatKiller_UI
{
    public partial class Config : Form
    {
        public static Config config;
        ArrayList displayStartTimeList;
        readonly ArrayList building_list = new ArrayList
        {
            new DictionaryEntry("1", "信息科学分馆"),
            new DictionaryEntry("2", "工学分馆"),
            new DictionaryEntry("3", "医学分馆"),
            new DictionaryEntry("4", "总馆")
        };
        readonly ArrayList startTimeList = new ArrayList
        {
            new DictionaryEntry("480", "8:00"),
            new DictionaryEntry("510", "8:30"),
            new DictionaryEntry("540", "9:00"),
            new DictionaryEntry("570", "9:30"),
            new DictionaryEntry("600", "10:00"),
            new DictionaryEntry("630", "10:30"),
            new DictionaryEntry("660", "11:00"),
            new DictionaryEntry("690", "11:30"),
            new DictionaryEntry("720", "12:00"),
            new DictionaryEntry("750", "12:30"),
            new DictionaryEntry("780", "13:00"),
            new DictionaryEntry("810", "13:30"),
            new DictionaryEntry("840", "14:00"),
            new DictionaryEntry("870", "14:30"),
            new DictionaryEntry("900", "15:00"),
            new DictionaryEntry("930", "15:30"),
            new DictionaryEntry("960", "16:00"),
            new DictionaryEntry("990", "16:30"),
            new DictionaryEntry("1020", "17:00"),
            new DictionaryEntry("1050", "17:30"),
            new DictionaryEntry("1080", "18:00"),
            new DictionaryEntry("1110", "18:30"),
            new DictionaryEntry("1140", "19:00"),
            new DictionaryEntry("1170", "19:30"),
            new DictionaryEntry("1200", "20:00"),
            new DictionaryEntry("1230", "20:30"),
            new DictionaryEntry("1260", "21:00"),
            new DictionaryEntry("1290", "21:30"),
            new DictionaryEntry("1320", "22:00")
        };
        public Config()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            config = this;
        }

        private void Config_Load(object sender, EventArgs e)
        {
            Text += " v" + Application.ProductVersion;

            textBox2.AppendText("Requesting for token.....success");
            textBox2.AppendText("\r\nFetching user information.....success");

            comboBox1.DataSource = building_list;
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            comboBox2.DisplayMember = "Value";
            comboBox2.ValueMember = "Key";

            comboBox3.DisplayMember = "Value";
            comboBox3.ValueMember = "Key";

            comboBox4.DisplayMember = "Value";
            comboBox4.ValueMember = "Key";

            comboBox5.DisplayMember = "Value";
            comboBox5.ValueMember = "Key";

            comboBox6.DisplayMember = "Value";
            comboBox6.ValueMember = "Key";

            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!IsHandleCreated)
            {
                Close();
            }
        }

        private void Config_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                label7.Enabled = true;
                textBox1.Enabled = true;
            }
            else
            {
                label7.Enabled = false;
                textBox1.Enabled = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    comboBox2.DataSource = new ArrayList
                    {
                        new DictionaryEntry("0", "(自动选择)"),
                        new DictionaryEntry("1", "只包含2~4楼和1楼云桌面"),
                        new DictionaryEntry("5", "一楼创新学习讨论区"),
                        new DictionaryEntry("4", "一楼3C创客空间"),
                        new DictionaryEntry("14", "3C创客-双屏电脑（20台）"),
                        new DictionaryEntry("15", "创新学习-MAC电脑（12台）"),
                        new DictionaryEntry("16", "创新学习-云桌面（42台）"),
                        new DictionaryEntry("6", "二楼西自然科学图书借阅区"),
                        new DictionaryEntry("7", "二楼东自然科学图书借阅区"),
                        new DictionaryEntry("8", "三楼西社会科学图书借阅区"),
                        new DictionaryEntry("10", "三楼东社会科学图书借阅区"),
                        new DictionaryEntry("12", "三楼自主学习区"),
                        new DictionaryEntry("9", "四楼西图书阅览区"),
                        new DictionaryEntry("11", "四楼东图书阅览区")
                    };
                    break;
                case 1:
                    comboBox2.DataSource = new ArrayList
                    {
                        new DictionaryEntry("0", "(自动选择)"),
                        new DictionaryEntry("19", "201室-东部自科图书借阅区"),
                        new DictionaryEntry("29", "2楼-中部走廊"),
                        new DictionaryEntry("31", "205室-中部电子阅览室笔记本区"),
                        new DictionaryEntry("32", "301室-东部自科图书借阅区"),
                        new DictionaryEntry("33", "305室-中部自科图书借阅区"),
                        new DictionaryEntry("34", "401室-东部自科图书借阅区"),
                        new DictionaryEntry("35", "405室中部期刊阅览区"),
                        new DictionaryEntry("37", "501室-东部外文图书借阅区"),
                        new DictionaryEntry("38", "505室-中部自科图书借阅区")
                    };
                    break;
                case 2:
                    comboBox2.DataSource = new ArrayList
                    {
                        new DictionaryEntry("0", "(自动选择)"),
                        new DictionaryEntry("20", "204教学参考书借阅区"),
                        new DictionaryEntry("21", "302中文科技图书借阅B区"),
                        new DictionaryEntry("23", "305科技期刊阅览区"),
                        new DictionaryEntry("24", "402中文文科图书借阅区"),
                        new DictionaryEntry("26", "502外文图书借阅区"),
                        new DictionaryEntry("27", "506医学人文阅览区")
                    };
                    break;
                case 3:
                    comboBox2.DataSource = new ArrayList
                    {
                        new DictionaryEntry("0", "(自动选择)"),
                        new DictionaryEntry("39", "A1-座位区"),
                        new DictionaryEntry("62", "A1-沙发区"),
                        new DictionaryEntry("66", "A1-苹果区"),
                        new DictionaryEntry("51", "A2"),
                        new DictionaryEntry("52", "A3"),
                        new DictionaryEntry("60", "A4"),
                        new DictionaryEntry("61", "A5"),
                        new DictionaryEntry("65", "B1"),
                        new DictionaryEntry("59", "B2"),
                        new DictionaryEntry("56", "B3"),
                        new DictionaryEntry("40", "C1自习区"),
                        new DictionaryEntry("92", "E1信息共享空间双屏云桌面区"),
                        new DictionaryEntry("84", "E2报刊阅览区"),
                        new DictionaryEntry("89", "E2大厅"),
                        new DictionaryEntry("85", "E3学位论文阅览区"),
                        new DictionaryEntry("86", "E4港台文献阅览区"),
                        new DictionaryEntry("87", "E5地方文献阅览区"),
                        new DictionaryEntry("88", "E6影印文献阅览区"),
                    };
                    break;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrayList seats = new ArrayList();
            if (comboBox2.SelectedValue.GetType() == typeof(string))
            {
                if (int.Parse(comboBox2.SelectedValue.ToString()) > 1)
                {
                    SeatKiller.GetSeats(comboBox2.SelectedValue.ToString(), seats);
                }
            }
            seats.Insert(0, new DictionaryEntry("0", "(自动选择)"));
            comboBox6.DataSource = seats;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            displayStartTimeList = new ArrayList();
            if (comboBox3.SelectedIndex == 0 && DateTime.Now.TimeOfDay.TotalMinutes > 480)
            {
                displayStartTimeList.Add(new DictionaryEntry("-1", "现在"));
                for (int i = 0; i < startTimeList.Count; i++)
                {
                    if (int.Parse(((DictionaryEntry)startTimeList[i]).Key.ToString()) > (int)DateTime.Now.TimeOfDay.TotalMinutes)
                    {
                        for (int j = i; j < startTimeList.Count; j++)
                        {
                            displayStartTimeList.Add((DictionaryEntry)startTimeList[j]);
                        }
                        break;
                    }
                }
            }
            else
            {
                displayStartTimeList = startTimeList;
            }
            comboBox4.DataSource = displayStartTimeList;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrayList endTimeList = new ArrayList();
            if (comboBox4.SelectedIndex == 0 && displayStartTimeList.Count == 30)
            {
                for (int i = comboBox4.SelectedIndex + 2; i <= 29; i++)
                {
                    endTimeList.Add(displayStartTimeList[i]);
                }
            }
            else
            {
                for (int i = comboBox4.SelectedIndex + 1; i <= displayStartTimeList.Count - 1; i++)
                {
                    endTimeList.Add(displayStartTimeList[i]);
                }
            }
            endTimeList.Add(new DictionaryEntry("1350", "22:30"));
            comboBox5.DataSource = endTimeList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "开始抢座")
            {
                Main.buildingId = comboBox1.SelectedValue.ToString();
                Main.roomId = comboBox2.SelectedValue.ToString() == "1" ? "0" : comboBox2.SelectedValue.ToString();
                Main.seatId = comboBox6.SelectedValue.ToString();
                Main.date = comboBox3.SelectedValue.ToString();
                Main.startTime = comboBox4.SelectedValue.ToString();
                Main.endTime = comboBox5.SelectedValue.ToString();

                SeatKiller.to_addr = textBox1.Text;
                SeatKiller.onlyPower = checkBox3.Checked;
                SeatKiller.onlyWindow = checkBox4.Checked;
                SeatKiller.onlyComputer = checkBox5.Checked;

                switch (Main.buildingId)
                {
                    case "1":
                        Main.rooms = (((IList)SeatKiller.xt_lite).Contains(Main.roomId) || comboBox2.SelectedValue.ToString() == "1") ? SeatKiller.xt_lite : SeatKiller.xt;
                        break;
                    case "2":
                        Main.rooms = SeatKiller.gt;
                        break;
                    case "3":
                        Main.rooms = SeatKiller.yt;
                        break;
                    case "4":
                        Main.rooms = SeatKiller.zt;
                        break;
                }

                label2.Enabled = false;
                label3.Enabled = false;
                label4.Enabled = false;
                label5.Enabled = false;
                label6.Enabled = false;
                label7.Enabled = false;
                label8.Enabled = false;
                label9.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                comboBox5.Enabled = false;
                comboBox6.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
                checkBox4.Enabled = false;
                checkBox5.Enabled = false;
                checkBox6.Enabled = false;
                textBox1.Enabled = false;
                pictureBox3.Enabled = false;
                pictureBox3.Image = Properties.Resources.description;
                button1.Text = "停止运行";

                Main.Start();
            }
            else
            {
                Main.Stop();

                label2.Enabled = true;
                label3.Enabled = true;
                label4.Enabled = true;
                label5.Enabled = true;
                label6.Enabled = true;
                label8.Enabled = true;
                label9.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                comboBox6.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
                checkBox4.Enabled = true;
                checkBox5.Enabled = true;
                checkBox6.Enabled = true;
                pictureBox3.Enabled = true;
                pictureBox3.Image = Properties.Resources.description_active;
                if (checkBox2.Checked)
                {
                    label7.Enabled = true;
                    textBox1.Enabled = true;
                }
                button1.Text = "开始抢座";
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (SeatKiller.GetUsrInf(false))
            {
                label1.Text = "你好 , " + SeatKiller.name + "  上次网页登录时间 : " + SeatKiller.last_login_time + "  状态 : " + SeatKiller.state + "  违约记录 : " + SeatKiller.violationCount + "次";
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (!SeatKiller.CheckResInf(true, true))
            {
                MessageBox.Show("暂无有效预约", "提示");
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("循环抢座功能需要保持软件开启，可用于每天自动抢上一次预约的座位，无需手动点击开始~", "帮助");
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int index = textBox2.GetFirstCharIndexOfCurrentLine();
            bool first = true;
            while (!backgroundWorker1.CancellationPending)
            {
                TimeSpan delta = SeatKiller.time.Subtract(DateTime.Now);
                if ((bool)e.Argument)
                {
                    if (first)
                    {
                        textBox2.AppendText("\r\n\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n");
                        first = false;
                    }
                    else
                    {
                        textBox2.Select(index, textBox2.TextLength - index);
                        textBox2.SelectedText = "\r\n\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n";
                    }
                }
                else
                {
                    if (first)
                    {
                        textBox2.AppendText("\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n");
                        first = false;
                    }
                    else
                    {
                        textBox2.Select(index, textBox2.TextLength - index);
                        textBox2.SelectedText = "\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n";
                    }
                }
                Thread.Sleep(1000);
            }
            e.Cancel = true;
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool flag = true;
            while (true)
            {
                if (SeatKiller.GetUsrInf(false))
                {
                    label1.Text = "你好 , " + SeatKiller.name + "  上次网页登录时间 : " + SeatKiller.last_login_time + "  状态 : " + SeatKiller.state + "  违约记录 : " + SeatKiller.violationCount + "次";
                    if (flag)
                    {
                        SeatKiller.GetNotice(true);
                        flag = false;
                    }
                }
                else
                {
                    Thread.Sleep(30000);
                    SeatKiller.GetToken(false);
                }
                Thread.Sleep(60000);
            }
        }

        private void backgroundWorker3_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                comboBox3.DataSource = new ArrayList
                {
                    new DictionaryEntry(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd") + " (今天)"),
                    new DictionaryEntry(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " (明天)")
                };
                comboBox3.SelectedIndex = 1;

                while (true)
                {
                    if (DateTime.Now.ToString("yyyy-MM-dd") != ((DictionaryEntry)comboBox3.Items[0]).Key.ToString())
                    {
                        break;
                    }
                    Thread.Sleep(10000);
                }
            }
        }

        private void backgroundWorker4_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            textBox2.AppendText("\r\n\r\n正在等待下一次循环...\r\n");
            while (true)
            {
                if (backgroundWorker4.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (DateTime.Now.TimeOfDay.TotalMinutes >= 1360 && DateTime.Now.TimeOfDay.TotalMinutes <= 1365)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            comboBox3.SelectedIndex = 1;
            Main.date = comboBox3.SelectedValue.ToString();
            Main.startTime = comboBox4.SelectedValue.ToString() == "-1" ? "480" : comboBox4.SelectedValue.ToString();
            textBox2.Text = "";
            Main.Start(true);
        }
    }
}