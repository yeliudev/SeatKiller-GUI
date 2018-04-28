using System;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace SeatKiller_UI
{
    public partial class Config : Form
    {
        public static Config config;
        public ArrayList startTime = new ArrayList();
        public Config()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            config = this;
        }

        private void Config_Load(object sender, EventArgs e)
        {
            textBox2.AppendText("Try getting token.....Status : success");

            SeatKiller.GetUsrInf();
            label1.Text = "你好 , " + SeatKiller.name + "  上次入馆时间 : " + SeatKiller.last_login_time + "  状态 : " + SeatKiller.state + "  违约记录 : " + SeatKiller.violationCount + "次";
            checkBox1.Checked = true;

            ArrayList building_list = new ArrayList();
            building_list.Add(new DictionaryEntry("1", "信息科学分馆"));
            building_list.Add(new DictionaryEntry("2", "工学分馆"));
            building_list.Add(new DictionaryEntry("3", "医学分馆"));
            building_list.Add(new DictionaryEntry("4", "总馆"));
            comboBox1.DataSource = building_list;
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            ArrayList date = new ArrayList();
            date.Add(new DictionaryEntry(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd") + " (今天)"));
            date.Add(new DictionaryEntry(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " (明天)"));
            comboBox3.DataSource = date;
            comboBox3.DisplayMember = "Value";
            comboBox3.ValueMember = "Key";
            comboBox3.SelectedIndex = 1;

            startTime.Add(new DictionaryEntry("480", "8:00"));
            startTime.Add(new DictionaryEntry("510", "8:30"));
            startTime.Add(new DictionaryEntry("540", "9:00"));
            startTime.Add(new DictionaryEntry("570", "9:30"));
            startTime.Add(new DictionaryEntry("600", "10:00"));
            startTime.Add(new DictionaryEntry("630", "10:30"));
            startTime.Add(new DictionaryEntry("660", "11:00"));
            startTime.Add(new DictionaryEntry("690", "11:30"));
            startTime.Add(new DictionaryEntry("720", "12:00"));
            startTime.Add(new DictionaryEntry("750", "12:30"));
            startTime.Add(new DictionaryEntry("780", "13:00"));
            startTime.Add(new DictionaryEntry("810", "13:30"));
            startTime.Add(new DictionaryEntry("840", "14:00"));
            startTime.Add(new DictionaryEntry("870", "14:30"));
            startTime.Add(new DictionaryEntry("900", "15:00"));
            startTime.Add(new DictionaryEntry("930", "15:30"));
            startTime.Add(new DictionaryEntry("960", "16:00"));
            startTime.Add(new DictionaryEntry("990", "16:30"));
            startTime.Add(new DictionaryEntry("1020", "17:00"));
            startTime.Add(new DictionaryEntry("1050", "17:30"));
            startTime.Add(new DictionaryEntry("1080", "18:00"));
            startTime.Add(new DictionaryEntry("1110", "18:30"));
            startTime.Add(new DictionaryEntry("1140", "19:00"));
            startTime.Add(new DictionaryEntry("1170", "19:30"));
            startTime.Add(new DictionaryEntry("1200", "20:00"));
            startTime.Add(new DictionaryEntry("1230", "20:30"));
            startTime.Add(new DictionaryEntry("1260", "21:00"));
            startTime.Add(new DictionaryEntry("1290", "21:30"));
            comboBox4.DataSource = startTime;
            comboBox4.DisplayMember = "Value";
            comboBox4.ValueMember = "Key";
            comboBox4.SelectedIndex = 0;
        }

        private void Config_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
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
                    ArrayList xt_list = new ArrayList();
                    xt_list.Add(new DictionaryEntry("0", "(自动选择)"));
                    xt_list.Add(new DictionaryEntry("1", "只包含2~4楼和1楼云桌面"));
                    xt_list.Add(new DictionaryEntry("5", "一楼创新学习讨论区"));
                    xt_list.Add(new DictionaryEntry("4", "一楼3C创客空间"));
                    xt_list.Add(new DictionaryEntry("14", "3C创客-双屏电脑（20台）"));
                    xt_list.Add(new DictionaryEntry("15", "创新学习-MAC电脑（12台）"));
                    xt_list.Add(new DictionaryEntry("16", "创新学习-云桌面（42台）"));
                    xt_list.Add(new DictionaryEntry("6", "二楼西自然科学图书借阅区"));
                    xt_list.Add(new DictionaryEntry("7", "二楼东自然科学图书借阅区"));
                    xt_list.Add(new DictionaryEntry("8", "三楼西社会科学图书借阅区"));
                    xt_list.Add(new DictionaryEntry("10", "三楼东社会科学图书借阅区"));
                    xt_list.Add(new DictionaryEntry("12", "三楼自主学习区"));
                    xt_list.Add(new DictionaryEntry("9", "四楼西图书阅览区"));
                    xt_list.Add(new DictionaryEntry("11", "四楼东图书阅览区"));
                    comboBox2.DataSource = xt_list;
                    comboBox2.DisplayMember = "Value";
                    comboBox2.ValueMember = "Key";
                    break;
                case 1:
                    ArrayList gt_list = new ArrayList();
                    gt_list.Add(new DictionaryEntry("0", "(自动选择)"));
                    gt_list.Add(new DictionaryEntry("19", "201室-东部自科图书借阅区"));
                    gt_list.Add(new DictionaryEntry("29", "2楼-中部走廊"));
                    gt_list.Add(new DictionaryEntry("31", "205室-中部电子阅览室笔记本区"));
                    gt_list.Add(new DictionaryEntry("32", "301室-东部自科图书借阅区"));
                    gt_list.Add(new DictionaryEntry("33", "305室-中部自科图书借阅区"));
                    gt_list.Add(new DictionaryEntry("34", "401室-东部自科图书借阅区"));
                    gt_list.Add(new DictionaryEntry("35", "405室中部期刊阅览区"));
                    gt_list.Add(new DictionaryEntry("37", "501室-东部外文图书借阅区"));
                    gt_list.Add(new DictionaryEntry("38", "505室-中部自科图书借阅区"));
                    comboBox2.DataSource = gt_list;
                    comboBox2.DisplayMember = "Value";
                    comboBox2.ValueMember = "Key";
                    break;
                case 2:
                    ArrayList yt_list = new ArrayList();
                    yt_list.Add(new DictionaryEntry("0", "(自动选择)"));
                    yt_list.Add(new DictionaryEntry("20", "204教学参考书借阅区"));
                    yt_list.Add(new DictionaryEntry("21", "302中文科技图书借阅B区"));
                    yt_list.Add(new DictionaryEntry("23", "305科技期刊阅览区"));
                    yt_list.Add(new DictionaryEntry("24", "402中文文科图书借阅区"));
                    yt_list.Add(new DictionaryEntry("26", "502外文图书借阅区"));
                    yt_list.Add(new DictionaryEntry("27", "506医学人文阅览区"));
                    comboBox2.DataSource = yt_list;
                    comboBox2.DisplayMember = "Value";
                    comboBox2.ValueMember = "Key";
                    break;
                case 3:
                    ArrayList zt_list = new ArrayList();
                    zt_list.Add(new DictionaryEntry("0", "(自动选择)"));
                    zt_list.Add(new DictionaryEntry("39", "A1-座位区"));
                    zt_list.Add(new DictionaryEntry("62", "A1-沙发区"));
                    zt_list.Add(new DictionaryEntry("66", "A1-苹果区"));
                    zt_list.Add(new DictionaryEntry("51", "A2"));
                    zt_list.Add(new DictionaryEntry("52", "A3"));
                    zt_list.Add(new DictionaryEntry("60", "A4"));
                    zt_list.Add(new DictionaryEntry("61", "A5"));
                    zt_list.Add(new DictionaryEntry("65", "B1"));
                    zt_list.Add(new DictionaryEntry("59", "B2"));
                    zt_list.Add(new DictionaryEntry("56", "B3"));
                    zt_list.Add(new DictionaryEntry("40", "C1自习区"));
                    comboBox2.DataSource = zt_list;
                    comboBox2.DisplayMember = "Value";
                    comboBox2.ValueMember = "Key";
                    break;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //About about = new About();
            //about.ShowDialog();
            MessageBox.Show("版本号：1.6.3\r\nGitHub仓库：https://github.com/goolhanrry/SeatKiller_UI\r\n还没搭好的个人主页：https://www.goolhanrry.club/\r\n\r\n本软件完全开源，也不会以任何形式收取捐赠\r\nCode Style写得一般，欢迎添加我的微信: aweawds 交流探讨或提交bug ۹(๑•̀ω•́ ๑)۶", "关于");
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrayList endTime = new ArrayList();
            int i;
            for (i = comboBox4.SelectedIndex + 1; i <= 27; i++)
            {
                endTime.Add(startTime[i]);
            }
            endTime.Add(new DictionaryEntry("1320", "22:00"));
            comboBox5.DataSource = endTime;
            comboBox5.DisplayMember = "Value";
            comboBox5.ValueMember = "Key";
            comboBox5.SelectedIndex = 0;
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
            comboBox6.DisplayMember = "Value";
            comboBox6.ValueMember = "Key";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "开始抢座")
            {
                main.buildingId = comboBox1.SelectedValue.ToString();
                switch (main.buildingId)
                {
                    case "1":
                        main.rooms = SeatKiller.xt;
                        break;
                    case "2":
                        main.rooms = SeatKiller.gt;
                        break;
                    case "3":
                        main.rooms = SeatKiller.yt;
                        break;
                    case "4":
                        main.rooms = SeatKiller.zt;
                        break;
                }

                if (comboBox1.SelectedValue.ToString() == "1" & comboBox2.SelectedValue.ToString() == "1")
                {
                    main.rooms = SeatKiller.xt_less;
                    main.roomId = "0";
                }
                else
                {
                    main.roomId = comboBox2.SelectedValue.ToString();
                }

                main.seatId = comboBox6.SelectedValue.ToString();
                main.date = comboBox3.SelectedValue.ToString();
                main.startTime = comboBox4.SelectedValue.ToString();
                main.endTime = comboBox5.SelectedValue.ToString();

                SeatKiller.to_addr = textBox1.Text;
                SeatKiller.onlyPower = checkBox3.Checked;
                SeatKiller.onlyWindow = checkBox4.Checked;
                SeatKiller.onlyComputer = checkBox5.Checked;

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
                textBox1.Enabled = false;
                button1.Text = "停止运行";

                main.Start();
            }
            else
            {
                main.Stop();
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

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int index = textBox2.GetFirstCharIndexOfCurrentLine();
            bool first = true;
            while (true)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
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
                        textBox2.Select(index, textBox2.TextLength - index - 1);
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
                        textBox2.Select(index, textBox2.TextLength - index - 1);
                        textBox2.SelectedText = "\r\n正在等待系统开放，剩余" + ((int)delta.TotalSeconds).ToString() + "秒\r\n";
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
