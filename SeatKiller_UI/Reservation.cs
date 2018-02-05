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
                if (SeatKiller.StopUsing())
                {
                    SeatKiller.exchange = false;
                }
                else
                {
                    MessageBox.Show("释放座位失败，请稍后重试", "失败");
                }
            }
            else
            {
                if (SeatKiller.CancelReservation(SeatKiller.res_id))
                {
                    SeatKiller.exchange = false;
                }
                else
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

        private void Reservation_Load(object sender, EventArgs e)
        {
            SeatKiller.exchange = true;
        }
    }
}
