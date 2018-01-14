using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Config : Form
    {
        public static Config config;
        public ArrayList startTime = new ArrayList();
        public Config()
        {
            InitializeComponent();
            config = this;
        }

        private void Config_Load(object sender, EventArgs e)
        {
            textBox2.AppendText("Try getting token.....Status : success");

            SeatKiller.GetUsrInf();
            label1.Text = "你好，" + SeatKiller.name + "  上次登录时间 : " + SeatKiller.last_login_time;
            checkBox1.Checked = true;

            ArrayList building_list = new ArrayList();
            building_list.Add(new DictionaryEntry("1", "信息科学分馆"));
            building_list.Add(new DictionaryEntry("2", "工学分馆"));
            building_list.Add(new DictionaryEntry("3", "医学分馆"));
            building_list.Add(new DictionaryEntry("4", "总馆"));
            comboBox1.DataSource = building_list;
            comboBox1.DisplayMember = "Value";

            comboBox3.Items.Add(DateTime.Now.ToString("yyyy-MM-dd")+" (今天)");
            comboBox3.Items.Add(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")+" (明天)");
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
                textBox1.Enabled = true;
            }
            else
            {
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
            MessageBox.Show("about", "关于");
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrayList endTime = new ArrayList();
            int i;
            for (i = comboBox4.SelectedIndex + 1; i <= 26; i++)
            {
                endTime.Add(startTime[i]);
            }
            endTime.Add(new DictionaryEntry("1320", "22:00"));
            comboBox5.DataSource = endTime;
            comboBox5.DisplayMember = "Value";
            comboBox5.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrayList seats = new ArrayList();
            SeatKiller.GetSeats(comboBox2.SelectedValue.ToString(), seats);
            seats.Insert(0, new DictionaryEntry("0", "(自动选择)"));
            comboBox6.DataSource = seats;
            comboBox6.DisplayMember = "Value";
            comboBox6.ValueMember = "Key";
        }
    }
}
