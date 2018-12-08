using System;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Reservation : Form
    {
        bool modal, exitFlag = true;
        public Reservation(bool modal)
        {
            this.modal = modal;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SeatKiller.reserving)
            {
                if (!SeatKiller.CancelReservation(SeatKiller.res_id, false))
                {
                    MessageBox.Show("取消预约失败，请稍后重试", "提示");
                }
            }
            else
            {
                if (!SeatKiller.StopUsing(false))
                {
                    MessageBox.Show("释放座位失败，请稍后重试", "提示");
                }
            }

            exitFlag = false;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            exitFlag = false;
            Close();
        }

        private void Reservation_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!modal && exitFlag)
            {
                Environment.Exit(0);
            }
            else if (!modal)
            {
                Config config = new Config();
                Hide();
                config.Show();
            }
        }
    }
}