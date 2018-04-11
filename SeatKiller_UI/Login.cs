using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Login : Form
    {
        BindingSource bs = new BindingSource();
        public Login()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            User.CreateSubKey();
            User.GetKey();

            bs.DataSource = User.users;
            comboBox1.DataSource = bs;
            if (comboBox1.Items.Count > 0)
            {
                User.users.Add("(清空登录信息)");
                bs.ResetBindings(false);
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SeatKiller.username = "";
            SeatKiller.password = "";
            while (true)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if (SeatKiller.GetToken(true) == "登录失败: 密码不正确")
                {
                    label4.Text = "Enable";
                    label4.ForeColor = Color.ForestGreen;
                }
                else
                {
                    label4.Text = "Unable";
                    label4.ForeColor = Color.Red;
                }
                Thread.Sleep(50);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            SeatKiller.username = comboBox1.Text;
            SeatKiller.password = textBox1.Text;
            string response = SeatKiller.GetToken(true);
            if (response == "Success")
            {
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
            else if (response == "Connection lost")
            {
                MessageBox.Show("登录失败，连接丢失", "登录失败");
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show(response, "登录失败");
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ActiveForm.Name != "Config" & ActiveForm.Name != "Reservation")
                Application.Exit();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text == "(清空登录信息)")
            {
                User.DeleteValue();
                bs.ResetBindings(false);
            }
            else
            {
                textBox1.Text = User.GetValue(comboBox1.Text);
            }
        }

        private void Login_Activated(object sender, EventArgs e)
        {
            comboBox1.Focus();
        }
    }
}
