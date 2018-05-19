using System;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
            label1.Text = "发现新版本，版本号 :  " + SeatKiller.newVersion + " ，大小约 " + SeatKiller.newVersionSize;
            textBox1.Text = SeatKiller.updateInfo;
        }

        private void Update_FormClosed(object sender, FormClosedEventArgs e)
        {
            Login.login.Enabled = true;
            Login.login.backgroundWorker1.RunWorkerAsync();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(SeatKiller.downloadURL);
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
