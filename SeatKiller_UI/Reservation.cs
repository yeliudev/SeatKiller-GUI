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

        private void button1_Click(object sender, EventArgs e)
        {
            if (SeatKiller.check_in)
            {
                if (!SeatKiller.StopUsing(true))
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

            Config config = new Config();
            Hide();
            config.Show();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Config config = new Config();
            Hide();
            config.Show();
            Close();
        }

        private void Reservation_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ActiveForm.Name != "Config")
                Application.Exit();
        }
    }
}
