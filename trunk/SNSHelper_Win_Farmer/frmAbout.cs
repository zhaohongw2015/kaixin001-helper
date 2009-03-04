using System.Diagnostics;
using System.Windows.Forms;

namespace SNSHelper_Win_Garden
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
            Process.Start("iexplore.exe", "http://kaixin001-helper.googlecode.com/files/Kaixin001.Helper.V1.0.Beta1.b.zip");
        }
    }
}
