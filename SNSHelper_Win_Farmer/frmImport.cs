using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SNSHelper.Kaixin001;
using SNSHelper_Win_Garden.Entity;
using System.Threading;

namespace SNSHelper_Win_Garden
{
    public partial class frmImport : DevComponents.DotNetBar.Office2007Form
    {
        public frmImport()
        {
            InitializeComponent();
        }

        GardenSetting gardenSetting;
        delegate void MethodWithParmString(string str);

        private void ShowMsgWhileImporting(string msg)
        {
            MethodWithParmString showMsgWileImporting = new MethodWithParmString(ShowMsg);

            this.Invoke(showMsgWileImporting, new object[] { msg });
        }
        private void ShowMsg(string msg)
        {
            txtBoard.AppendText(msg);
        }

        private void frmImport_Load(object sender, EventArgs e)
        {
            gardenSetting = GardenSetting.LoadGardenSetting(Application.StartupPath);
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);

            txtFilePath.Text = openFileDialog1.FileName;

            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                btnImport.Enabled = true;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.FileName) || !File.Exists(openFileDialog1.FileName))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "文件不存在！", "提示");
                return;
            }

            importedFile = openFileDialog1.FileName;

            btnChooseFile.Enabled = false;
            btnImport.Enabled = false;
            txtFilePath.Enabled = false;

            importThread = new Thread(BeginImport);
            importThread.Start();
        }

        Thread importThread;
        private string importedFile = string.Empty;

        private void BeginImport()
        {
            try
            {
                StreamReader sr = new StreamReader(importedFile);

                Dictionary<string, string> accountPasswordList = new Dictionary<string, string>();

                string[] temp;
                while (sr.Peek() != -1)
                {
                    temp = sr.ReadLine().Replace("，", ",").Split(',');

                    if (temp.Length >= 2)
                    {
                        accountPasswordList.Add(temp[0].Trim(), temp[1].Trim());
                    }
                }

                sr.Dispose();
                sr = null;

                GardenHelper helper;
                foreach (string email in accountPasswordList.Keys)
                {
                    Utility utility = new Utility();
                    ShowMsgWhileImporting(string.Format("{0} 正在登录...\r\n", email));
                    if (!utility.Login(email, accountPasswordList[email]))
                    {
                        ShowMsgWhileImporting("登录失败！\r\n\r\n");

                        continue;
                    }

                    helper = new GardenHelper(utility, gardenSetting.GlobalSetting.NetworkDelay);

                    ShowMsgWhileImporting("登录成功！正在加载好友数据...\r\n");
                    Dictionary<string, string> friendList = helper.GetGardenFriend();

                    gardenSetting.AccountSettings.Add(InitAccountSetting(email, accountPasswordList[email], friendList));
                    ShowMsgWhileImporting("好友数据处理完毕！正在退出...\r\n");

                    utility.Logout();

                    ShowMsgWhileImporting("成功退出！\r\n\r\n");
                }

                GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);

                ShowMsgWhileImporting(string.Format("帐号导入完毕！共导入 {0} 个帐号！", accountPasswordList.Count));
            }
            catch (Exception ex)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "读取文件失败！", "提示");
            }
            finally
            {
                MethodInvoker mi = new MethodInvoker(StopImport);
                this.Invoke(mi);

                importThread = null;
            }
        }

        private void StopImport()
        {
            txtFilePath.Enabled = true;
            btnChooseFile.Enabled = true;
            btnImport.Enabled = true;
            DevComponents.DotNetBar.MessageBoxEx.Show(this, "导入操作已完成！", "提示");
        }

        private List<FriendSetting> InitFriendSettings(Dictionary<string, string> friendList)
        {
            List<FriendSetting> list = new List<FriendSetting>();

            foreach (string uid in friendList.Keys)
            {
                FriendSetting etFriendSetting = new FriendSetting();

                etFriendSetting.UID = uid;
                etFriendSetting.Name = friendList[uid];

                list.Add(etFriendSetting);
            }

            return list;
        }

        private AccountSetting InitAccountSetting(string loginEmail, string password, Dictionary<string, string> friendList)
        {
            AccountSetting etAccountSetting = new AccountSetting();
            etAccountSetting.LoginEmail = loginEmail;
            etAccountSetting.LoginPassword = password;
            etAccountSetting.AutoBuySeed = true;
            etAccountSetting.AutoFarm = true;
            etAccountSetting.AutoGrass = true;
            etAccountSetting.AutoHavest = true;
            etAccountSetting.AutoPlough = true;
            etAccountSetting.AutoVermin = true;
            etAccountSetting.AutoWater = true;
            etAccountSetting.Crops = "胡萝卜";
            etAccountSetting.FriendWaterLowLimit = "0";
            etAccountSetting.IsOperate = true;
            etAccountSetting.Seed = "胡萝卜";
            etAccountSetting.WaterLowLimit = "4";
            etAccountSetting.FriendSettings = InitFriendSettings(friendList);

            return etAccountSetting;
        }

        private void frmImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (importThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show(this, "正在导入帐号，确定要退出吗？退出将导致导入不成功！", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    importThread = null;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
