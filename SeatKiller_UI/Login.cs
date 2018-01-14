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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SeatKiller.username = textBox1.Text;
            SeatKiller.password = textBox2.Text;
            string response = SeatKiller.GetToken(true);
            if (response == "success")
            {
                Config config = new Config();
                config.Show();
                Close();
            }
            else if (response == "Connection lost")
            {
                MessageBox.Show("登录失败，连接丢失", "登录失败");
            }
            else
            {
                MessageBox.Show(response, "登录失败");
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ActiveForm.Name != "Config")
                Application.Exit();
        }
    }
}
