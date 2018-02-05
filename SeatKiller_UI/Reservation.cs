using System;
using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class Reservation : Form
    {
        public static Reservation reservation;
        public Reservation()
        {
            InitializeComponent();
            reservation = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SeatKiller.check_in)
            {
                if (!SeatKiller.StopUsing())
                {
                    MessageBox.Show("释放座位失败，请稍后重试", "失败");
                }
            }
            else
            {
                if (!SeatKiller.CancelReservation(SeatKiller.res_id))
                {
                    MessageBox.Show("取消预约失败，请稍后重试", "失败");
                }
            }
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
