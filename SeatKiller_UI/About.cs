using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/goolhanrry/SeatKiller_UI");
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
