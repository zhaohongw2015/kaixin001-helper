using System.Diagnostics;
using System.Windows.Forms;

namespace SNSHelper_Win_Parker
{
    public partial class frmAbout : DevComponents.DotNetBar.Office2007Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("iexplore.exe", "jailu.cnblogs.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("iexplore.exe", "http://code.google.com/p/kaixin001-helper/");
        }
    }
}
