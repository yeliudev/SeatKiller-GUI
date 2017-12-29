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
            textBox3.Text = SK.GetToken();
        }
    }
}
