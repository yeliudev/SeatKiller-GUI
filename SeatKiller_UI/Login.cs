using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Login : Form
    {
        public static Login login;
        BindingSource bs = new BindingSource();
        bool exitFlag = true;
        public Login()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            login = this;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            Text += " v" + Application.ProductVersion;

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();
            User.CreateSubKey();
            User.GetKey();

            bs.DataSource = User.users;
            comboBox1.DataSource = bs;
            if (comboBox1.Items.Count > 0)
            {
                User.users.Add("（清空登录信息）");
                bs.ResetBindings(false);
            }
        }

        private void Login_Activated(object sender, EventArgs e)
        {
            comboBox1.Focus();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "（清空登录信息）")
            {
                User.DeleteValue();
                bs.ResetBindings(false);
            }
            else
            {
                textBox1.Text = User.GetValue(comboBox1.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            SeatKiller.username = comboBox1.Text;
            SeatKiller.password = textBox1.Text;
            string response = SeatKiller.GetToken(false);
            if (response == "Success")
            {
                exitFlag = false;
                if (checkBox1.Checked)
                {
                    User.SetValue(comboBox1.Text, textBox1.Text);
                }

                Hide();
                if (!SeatKiller.CheckResInf())
                {
                    Config config = new Config();
                    config.Show();
                }
                Close();
            }
            else if (response == "System Maintenance")
            {
                MessageBox.Show("系统维护中（23:45 ~ 0:15）", "登录失败");
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            else if (response == "Connection lost")
            {
                MessageBox.Show("连接丢失，请稍后重试", "登录失败");
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            else
            {
                MessageBox.Show(response, "提示");
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Dispose();
            }
            catch { }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }
            else if (exitFlag)
            {
                Environment.Exit(0);
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SeatKiller.username = "";
            SeatKiller.password = "";
            while (!backgroundWorker1.CancellationPending)
            {
                if (SeatKiller.GetToken(false) == "登录失败: 用户名或密码不正确")
                {
                    backgroundWorker1.ReportProgress(100);
                }
                else
                {
                    backgroundWorker1.ReportProgress(0);
                }
                Thread.Sleep(200);
            }
            e.Cancel = true;
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                try
                {
                    label4.Text = "Enable";
                    label4.ForeColor = Color.ForestGreen;
                }
                catch { }
            }
            else
            {
                try
                {
                    label4.Text = "Unable";
                    label4.ForeColor = Color.Red;
                }
                catch { }
            }
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (SeatKiller.CheckUpdate())
            {
                backgroundWorker1.CancelAsync();
                backgroundWorker2.ReportProgress(100);
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Update update = new Update();
            update.ShowDialog();
        }

        private void backgroundWorker3_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SeatKiller.GetNotice();
        }
    }
}