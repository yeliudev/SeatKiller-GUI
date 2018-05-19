using System;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Reservation : Form
    {
        public Reservation()
        {
            InitializeComponent();
        }

        private void Reservation_Load(object sender, EventArgs e)
        {
            SeatKiller.exitFlag = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SeatKiller.check_in)
            {
                if (!SeatKiller.StopUsing(false))
                {
                    MessageBox.Show("释放座位失败，请稍后重试", "失败");
                }
            }
            else
            {
                if (!SeatKiller.CancelReservation(SeatKiller.res_id, false))
                {
                    MessageBox.Show("取消预约失败，请稍后重试", "失败");
                }
            }

            SeatKiller.exitFlag = true;
            Config config = new Config();
            Hide();
            config.Show();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SeatKiller.exitFlag = true;
            Config config = new Config();
            Hide();
            config.Show();
            Close();
        }

        private void Reservation_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!SeatKiller.exitFlag)
            {
                Application.Exit();
            }
        }
    }
}
