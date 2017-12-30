using System;
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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            SeatKiller test = new SeatKiller("", "");
            if (test.GetToken() == "fail")
            {
                label4.Text = "Enable";
                label4.ForeColor = Color.ForestGreen;
            }
            else
            {
                label4.Text = "Unable";
                label4.ForeColor = Color.Red;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SeatKiller SK = new SeatKiller(textBox1.Text, textBox2.Text);
            string response = SK.GetToken();
            if (response == "fail")
            {
                MessageBox.Show("登录失败，请检查用户名和密码", "登录失败");
            }
            else if (response == "Connection lost")
            {
                MessageBox.Show("登录失败，连接丢失", "登录失败");
            }
            else
            {
                Config config = new Config();
                config.Show();
                Close();
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ActiveForm.Name != "config")
                Application.Exit();
        }
    }
}
