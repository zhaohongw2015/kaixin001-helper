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

namespace SNSHelper_Win_Garden
{
    public partial class frmImport : DevComponents.DotNetBar.Office2007Form
    {
        public frmImport()
        {
            InitializeComponent();
        }

        GardenSetting gardenSetting;
        private void frmImport_Load(object sender, EventArgs e)
        {
            gardenSetting = GardenSetting.LoadGardenSetting();
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

            try
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);

                Dictionary<string, string> accountPasswordList = new Dictionary<string, string>();

                string[] temp;
                while (sr.Peek() != -1)
                {
                    temp = sr.ReadLine().Replace("，", ",").Split(',');

                    if (temp.Length >= 2)
                    {
                        accountPasswordList.Add(temp[0], temp[1]);
                    }
                }

                sr.Dispose();
                sr = null;

                GardenHelper helper = new GardenHelper();
                foreach (string email in accountPasswordList.Keys)
                {
                    txtBoard.AppendText("{0} 正在登录...\r\n");
                    if (!Utility.Login(email, accountPasswordList[email]))
                    {
                        txtBoard.AppendText("登录失败！\r\n\r\n");

                        continue;
                    }

                    txtBoard.AppendText("登录成功！正在加载好友数据...\r\n");
                    Dictionary<string, string> friendList = helper.GetGardenFriend();

                    gardenSetting.AccountSettings.Add(InitAccountSetting(email, accountPasswordList[email], friendList));
                    txtBoard.AppendText("好友数据处理完毕！正在退出...\r\n");

                    Utility.Logout();

                    txtBoard.AppendText("成功退出！\r\n\r\n");
                }

                GardenSetting.SaveGardenSetting(gardenSetting);

                txtBoard.AppendText(string.Format("帐号导入完毕！共导入 {0} 个帐号！", accountPasswordList.Count));
            }
            catch(Exception ex)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "读取文件失败！", "提示");
            }
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
    }
}
