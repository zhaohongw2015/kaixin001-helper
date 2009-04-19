using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SNSHelper.Kaixin001;
using SNSHelper_Win_Parker.Entity;
using SNSHelper_Win_Parker.Helper;

namespace SNSHelper_Win_Parker
{
    public partial class frmImport : DevComponents.DotNetBar.Office2007Form
    {
        public frmImport()
        {
            InitializeComponent();
        }

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
            ConfigHelper.LoadConfig();
        }

        Thread importThread;
        private string importedFile = string.Empty;

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
                foreach (string email in accountPasswordList.Keys)
                {
                    Utility utility = new Utility();
                    ShowMsgWhileImporting(string.Format("{0} 正在登录...\r\n", email));
                    if (!utility.Login(email, accountPasswordList[email]))
                    {
                        ShowMsgWhileImporting("登录失败！\r\n\r\n");

                        continue;
                    }

                    ShowMsgWhileImporting("登录成功！正在加载好友数据...\r\n");

                    ParkingHelper helper = new ParkingHelper(utility);

                    AccountSetting temp_currentAccountSetting = new AccountSetting();



                    for (int i = 0; i < helper.ParkerFriendList.Count; i++)
                    {

                        temp_currentAccountSetting.FriendSettings.Add(new FriendSetting(helper.ParkerFriendList[i].RealName, helper.ParkerFriendList[i].UId, helper.ParkerFriendList[i].SceneMoney));
                       
                    }
                    //高级车位优先停车
                    for (int i = 0; i < temp_currentAccountSetting.FriendSettings.Count; i++)
                    {

                        temp_currentAccountSetting.FriendSettings[i].AllowedPark = true;
                        temp_currentAccountSetting.FriendSettings[i].AllowedPost = true;
                        
                        if ((Convert.ToInt32(temp_currentAccountSetting.FriendSettings[i].Scenemoney) > 10) && (temp_currentAccountSetting.FriendSettings[i].ParkPriority == int.MaxValue))
                        {
                            temp_currentAccountSetting.FriendSettings[i].ParkPriority = 21 - Convert.ToInt32(temp_currentAccountSetting.FriendSettings[i].Scenemoney);
                        }
                    }
                    temp_currentAccountSetting.IsOperation = true;
                    temp_currentAccountSetting.LoginEmail = email;
                    temp_currentAccountSetting.LoginPwd = accountPasswordList[email];
                    temp_currentAccountSetting.MinPark = 1000;
                    temp_currentAccountSetting.MinPost = 100;
                    temp_currentAccountSetting.AdvanceSettings.AutoBuyCar = false;
                    temp_currentAccountSetting.AdvanceSettings.AutoUpdateCar = false;
                    temp_currentAccountSetting.AdvanceSettings.MaxCarNo = 0;

                    ConfigHelper.AccountSettings.Add(temp_currentAccountSetting);

                  
                    ShowMsgWhileImporting("好友数据处理完毕！正在退出...\r\n");

                    utility.Logout();

                    ShowMsgWhileImporting("成功退出！\r\n\r\n");
                }
               
              
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
