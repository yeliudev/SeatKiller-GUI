using System.Windows.Forms;

namespace SeatKiller_UI
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void About_Load(object sender, System.EventArgs e)
        {
            label1.Text += Application.ProductVersion;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void pictureBox2_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/c1aris/SeatKiller_UI");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (SeatKiller.CheckUpdate())
            {
                Cursor = Cursors.Default;
                Update update = new Update();
                update.ShowDialog();
            }
            else
            {
                Cursor = Cursors.Default;
                MessageBox.Show("当前版本已是最新版", "提示");
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/c1aris/SeatKiller_UI/issues/new");
        }
    }
}
