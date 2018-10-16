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
            label1.Text = "版本号：" + Application.ProductVersion + "\r\nGitHub仓库：\r\n不定时更新，最新版本将在上方GitHub仓库中发布\r\n\r\n本软件完全开源，也不会以任何形式收取捐赠\r\nCode Style写得一般，欢迎添加我的微信：aweawds 交流探讨或提交bug ۹(๑•̀ω•́ ๑)۶";
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
