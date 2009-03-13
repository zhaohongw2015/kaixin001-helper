using System;
using System.Collections.Generic;
using System.Text;
using SNSHelper.Kaixin001.Entity.Garden;
using SNSHelper_Win_Garden.Entity;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SNSHelper.Kaixin001;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        /// <summary>
        /// 标记当前操作是否为新增帐号
        /// </summary>
        private bool isNewAccountFlag = false;

        /// <summary>
        /// 
        /// </summary>
        private void InitSetting()
        {
            isNewAccountFlag = true;

            ShowSeedData();

            InitAccountSetting();

            gardenSetting = GardenSetting.LoadGardenSetting(Application.StartupPath);

            ShowGlobalSettings();

            ShowAccountInList(gardenSetting.AccountSettings);
        }

        /// <summary>
        /// 显示全局设置
        /// </summary>
        private void ShowGlobalSettings()
        {
            txtParkingInterval.Value = gardenSetting.GlobalSetting.WorkingInterval;
            txtNetDelay.Value = gardenSetting.GlobalSetting.NetworkDelay / 1000 <= txtNetDelay.MinValue ? txtNetDelay.MinValue : gardenSetting.GlobalSetting.NetworkDelay / 1000;
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            isNewAccountFlag = true;

            InitAccountSetting();
        }

        #region Show Seed Data

        private string GetSeedName(string seedId)
        {
            foreach (SeedItem item in seedData.SeedItems)
            {
                if (item.SeedID.Equals(seedId))
                {
                    return item.Name;
                }
            }

            return string.Empty;
        }

        private string GetSeedID(string seedName)
        {
            foreach (SeedItem item in seedData.SeedItems)
            {
                if (item.Name.Equals(seedName))
                {
                    return item.SeedID;
                }
            }

            return string.Empty;
        }

        private double GetSeedPrice(string seedName)
        {
            foreach (SeedItem item in seedData.SeedItems)
            {
                if (item.Name.Equals(seedName))
                {
                    return item.Price;
                }
            }

            return 0;
        }

        SeedData seedData;

        /// <summary>
        /// 在相关下拉控件中现在种子信息
        /// </summary>
        private void ShowSeedData()
        {
            string xml = File.ReadAllText(Path.Combine(Application.StartupPath, "SeedData.xml"), Encoding.UTF8);
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\" ?>", "");

            seedData = new SeedData(xml);

            foreach (SeedItem item in seedData.SeedItems)
            {
                cbxCrops.Items.Add(item.Name);
                cbxStealCrops.Items.Add(item.Name);
            }
        }

        #endregion

        #region Load friend from net



        private void btnLoadFriend_Click(object sender, EventArgs e)
        {
            //if (farmerWorkingThread != null)
            //{
            //    DevComponents.DotNetBar.MessageBoxEx.Show("农夫定时任务正在运行，暂时不能进行账户导入工作！", "提示");
            //    return;
            //}
            if (string.IsNullOrEmpty(txtNewLoginEmail.Text.Trim()) || string.IsNullOrEmpty(txtNewLoginPwd.Text.Trim()))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("请输入完整的帐号信息！");
                return;
            }

            BeginGetNetFriend();
        }

        private void BeginGetNetFriend()
        {
            dgvFriendList.ScrollBars = ScrollBars.None;
            groupPanel2.Enabled = false;
            groupPanel5.Visible = true;

            ShowLoadNetFriendStatus("正在登录...");

            getNetFriendThread = new Thread(LoadNetFriend);
            getNetFriendThread.Start();
        }

        private void StopGetNetFriend()
        {
            dgvFriendList.ScrollBars = ScrollBars.Both;
            groupPanel2.Enabled = true;
            groupPanel5.Visible = false;
        }

        private void StopGetNetFriendWhileLoadingNetFriend()
        {
            MethodWithoutParm stopGetNetFriend = new MethodWithoutParm(StopGetNetFriend);
            this.Invoke(stopGetNetFriend);
        }

        private void ShowLoadNetFriendStatus(string msg)
        {
            lblNetworkingStatus.Text = msg;
        }

        delegate void MethodWithParmString(string str);
        delegate void MethodWithParmDictionary(string loginEmail, Dictionary<string, string> dictionary);
        delegate void MethodWithoutParm();

        private void LoadNetFriend()
        {
            MethodWithParmString showLoadNetFriendStatus = new MethodWithParmString(ShowLoadNetFriendStatus);

            Utility utility = new Utility();
            if (utility.Login(txtNewLoginEmail.Text, txtNewLoginPwd.Text))
            {
                this.BeginInvoke(showLoadNetFriendStatus, new object[] { "登录成功！正在获取好友数据..." });
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("登录失败！");

                StopGetNetFriendWhileLoadingNetFriend();

                return;
            }

            GardenHelper helper = new GardenHelper(utility, gardenSetting.GlobalSetting.NetworkDelay);

            Dictionary<string, string> friends = helper.GetGardenFriend();

            this.BeginInvoke(showLoadNetFriendStatus, new object[] { "好友数据获取完毕！正在退出..." });

            utility.Logout();

            StopGetNetFriendWhileLoadingNetFriend();

            MethodWithParmDictionary showNetFriend = new MethodWithParmDictionary(ShowNetFriend);
            this.BeginInvoke(showNetFriend, new object[] { txtNewLoginEmail.Text, friends });
        }

        private void ShowNetFriend(string loginEmail, Dictionary<string, string> friends)
        {
            currentConfiguringAccountSetting = GetAccountSetting(gardenSetting.AccountSettings, loginEmail);

            if (currentConfiguringAccountSetting == null)
            {
                currentConfiguringAccountSetting = new SNSHelper_Win_Garden.Entity.AccountSetting();
            }

            foreach (string uid in friends.Keys)
            {
                if (GetFriendSetting(currentConfiguringAccountSetting.FriendSettings, uid) == null)
                {
                    FriendSetting etFriendSetting = new FriendSetting();
                    etFriendSetting.UID = uid;
                    etFriendSetting.Name = friends[uid];

                    currentConfiguringAccountSetting.FriendSettings.Add(etFriendSetting);
                }
            }

            ShowFriendSettings(currentConfiguringAccountSetting.FriendSettings);
        }

        private void ShowFriendSettings(List<FriendSetting> friendSettings)
        {
            dgvFriendList.Rows.Clear();

            foreach (FriendSetting item in friendSettings)
            {
                dgvFriendList.Rows.Add(item.Name, item.UID, item.Steal, item.Plough, item.Farm, item.Water, item.Vermin, item.Grass);
            }
        }
        #endregion

        #region 帐号花园设置

        GardenSetting gardenSetting;
        /// <summary>
        /// 当前正在配置的帐号
        /// </summary>
        SNSHelper_Win_Garden.Entity.AccountSetting currentConfiguringAccountSetting;

        /// <summary>
        /// 初始化帐号设置
        /// </summary>
        private void InitAccountSetting()
        {
            txtNewLoginEmail.Clear();
            txtNewLoginPwd.Clear();

            dgvFriendList.Rows.Clear();

            ckxAutoFarm.Checked = true;
            cbxCrops.Enabled = true;
            cbxCrops.SelectedIndex = 0;
            ckxIsUsingPrivateSetting.Enabled = true;
            ckxIsUsingPrivateSetting.Checked = false;

            ckxAutoWater.Checked = true;
            cbxWater.Enabled = true;
            cbxWater.SelectedIndex = 0;
            cbxFriendWater.SelectedIndex = 4;

            ckxAutoVermin.Checked = true;
            ckxAutoPlough.Checked = true;

            ckxAutoBuySeed.Checked = true;

            ckxAutoHavest.Checked = true;
            ckxAutoHavestInTime.Checked = true;
            ckxAutoStealInTime.Checked = true;

            ckxIsCare.Checked = true;

            ckxAutoGrass.Checked = true;

            ckbAutoSell.Checked = true;
        }

        private void ckxAutoFarm_CheckedChanged(object sender, EventArgs e)
        {
            cbxCrops.Enabled = ckxAutoFarm.Checked;
            ckxIsUsingPrivateSetting.Enabled = ckxAutoFarm.Checked;
        }

        private void ckxAutoWater_CheckedChanged(object sender, EventArgs e)
        {
            cbxWater.Enabled = ckxAutoWater.Checked;
        }

        #endregion

        #region Modify the garden setting

        private void dgvFriendList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 2:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Steal = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 3:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Plough = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 4:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Farm = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 5:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Water = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 6:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Vermin = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 7:
                    currentConfiguringAccountSetting.FriendSettings[e.RowIndex].Grass = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region 帐号列表相关

        /// <summary>
        /// 在帐号列表中显示已添加到帐号
        /// </summary>
        /// <param name="accountSettings"></param>
        private void ShowAccountInList(List<SNSHelper_Win_Garden.Entity.AccountSetting> accountSettings)
        {
            lsbAccount.Items.Clear();

            for (int i = 0; i < accountSettings.Count; i++)
            {
                lsbAccount.Items.Add(accountSettings[i].LoginEmail);
            }
        }

        private void lsbAccount_DoubleClick(object sender, EventArgs e)
        {
            if (lsbAccount.SelectedItems.Count != 0)
            {
                ShowAccountSetting(GetAccountSetting(gardenSetting.AccountSettings, lsbAccount.SelectedItems[0].Text));
            }
        }

        private void btiLoadAccountSetting_Click(object sender, EventArgs e)
        {
            if (lsbAccount.SelectedItems.Count != 0)
            {
                ShowAccountSetting(GetAccountSetting(gardenSetting.AccountSettings, lsbAccount.SelectedItems[0].Text));
            }
        }

        /// <summary>
        /// 显示帐号配置信息
        /// </summary>
        /// <param name="accountSetting"></param>
        private void ShowAccountSetting(SNSHelper_Win_Garden.Entity.AccountSetting accountSetting)
        {
            txtNewLoginEmail.Text = accountSetting.LoginEmail;
            txtNewLoginPwd.Text = accountSetting.LoginPassword;
            cbxIsOperation.Checked = accountSetting.IsOperate;

            ShowFriendSettings(accountSetting.FriendSettings);

            ckxAutoFarm.Checked = accountSetting.AutoFarm;
            cbxCrops.Enabled = accountSetting.AutoFarm;
            cbxCrops.Text = accountSetting.Crops;
            ckxIsUsingPrivateSetting.Enabled = accountSetting.AutoFarm;
            ckxIsUsingPrivateSetting.Checked = accountSetting.IsUsingPrivateSetting;

            ckxAutoWater.Checked = accountSetting.AutoWater;
            cbxWater.Enabled = accountSetting.AutoWater;
            cbxWater.Text = string.IsNullOrEmpty(accountSetting.WaterLowLimit) ? "4" : Convert.ToInt32(accountSetting.WaterLowLimit) >= 5 ? "4" : accountSetting.WaterLowLimit;

            cbxFriendWater.Text = Convert.ToInt32(accountSetting.FriendWaterLowLimit) >= 5 ? "4" : accountSetting.FriendWaterLowLimit;

            ckxAutoPlough.Checked = accountSetting.AutoPlough;
            ckxAutoVermin.Checked = accountSetting.AutoVermin;

            ckxAutoBuySeed.Checked = accountSetting.AutoBuySeed;

            ckxAutoHavest.Checked = accountSetting.AutoHavest;
            ckxAutoHavestInTime.Checked = accountSetting.AutoHavestInTime;
            ckxAutoStealInTime.Checked = accountSetting.AutoStealInTime;

            ckxIsCare.Checked = accountSetting.IsCare;

            ckxAutoGrass.Checked = accountSetting.AutoGrass;

            ckbAutoSell.Checked = accountSetting.AutoSell;

            cbxStealCrops.Text = accountSetting.StealCrops;

            currentConfiguringAccountSetting = accountSetting;
            isNewAccountFlag = false;
        }

        private void btiDeleteAccount_Click(object sender, EventArgs e)
        {
            if (lsbAccount.SelectedItems.Count != 0)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("请确定要删除这个帐号吗？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    DeleteAccountSetting(GetAccountSetting(gardenSetting.AccountSettings, lsbAccount.SelectedItems[0].Text));
                }
            }
        }

        private void DeleteAccountSetting(SNSHelper_Win_Garden.Entity.AccountSetting accountSetting)
        {
            gardenSetting.AccountSettings.Remove(accountSetting);

            GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);

            lsbAccount.Items.Remove(lsbAccount.SelectedItems[0]);
        }

        private void buttonItem7_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            if (lsbAccount.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            if (lsbAccount.SelectedItems[0].Index == 0)
            {
                btiUp.Enabled = false;
            }
            else
            {
                btiUp.Enabled = true;
            }

            if (lsbAccount.SelectedItems[0].Index == lsbAccount.Items.Count - 1)
            {
                btiDown.Enabled = false;
            }
            else
            {
                btiDown.Enabled = true;
            }
        }

        private void btiUp_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;

            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Insert(index - 1, lvi);

            SNSHelper_Win_Garden.Entity.AccountSetting temp = gardenSetting.AccountSettings[index];
            gardenSetting.AccountSettings[index] = gardenSetting.AccountSettings[index - 1];
            gardenSetting.AccountSettings[index - 1] = temp;

            GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);
            InitAccountSetting();

            currentConfiguringAccountSetting = null;
        }

        private void btiDown_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;

            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Insert(index + 1, lvi);

            SNSHelper_Win_Garden.Entity.AccountSetting temp = gardenSetting.AccountSettings[index];
            gardenSetting.AccountSettings[index] = gardenSetting.AccountSettings[index + 1];
            gardenSetting.AccountSettings[index + 1] = temp;

            GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);

            currentConfiguringAccountSetting = null;
            InitAccountSetting();
        }

        private void btnSaveGlobalSetting_Click(object sender, EventArgs e)
        {
            gardenSetting.GlobalSetting.WorkingInterval = txtParkingInterval.Value;
            gardenSetting.GlobalSetting.NetworkDelay = txtNetDelay.Value * 1000;

            GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);

            DevComponents.DotNetBar.MessageBoxEx.Show("保存设置成功！", "提示");

            //Utility.NetworkDelay = gardenSetting.GlobalSetting.NetworkDelay;
        }

        #endregion

        #region GardenSetting辅助方法

        private SNSHelper_Win_Garden.Entity.AccountSetting GetAccountSetting(List<SNSHelper_Win_Garden.Entity.AccountSetting> accountSettings, string loginEmail)
        {
            foreach (SNSHelper_Win_Garden.Entity.AccountSetting item in accountSettings)
            {
                if (item.LoginEmail.Equals(loginEmail))
                {
                    return item;
                }
            }

            return null;
        }

        private FriendSetting GetFriendSetting(List<FriendSetting> friendSettings, string uid)
        {
            foreach (FriendSetting item in friendSettings)
            {
                if (item.UID.Equals(uid))
                {
                    return item;
                }
            }

            return null;
        }

        #endregion

        private void btnSaveAccount_Click(object sender, EventArgs e)
        {
            if (currentConfiguringAccountSetting == null)
            {
                return;
            }

            currentConfiguringAccountSetting.LoginEmail = txtNewLoginEmail.Text;
            currentConfiguringAccountSetting.LoginPassword = txtNewLoginPwd.Text;

            currentConfiguringAccountSetting.IsOperate = cbxIsOperation.Checked;

            currentConfiguringAccountSetting.AutoFarm = ckxAutoFarm.Checked;
            currentConfiguringAccountSetting.Crops = ckxAutoFarm.Checked ? cbxCrops.Text : string.Empty;
            currentConfiguringAccountSetting.IsUsingPrivateSetting = ckxAutoFarm.Checked ? ckxIsUsingPrivateSetting.Checked : false;

            currentConfiguringAccountSetting.AutoWater = ckxAutoWater.Checked;
            currentConfiguringAccountSetting.WaterLowLimit = ckxAutoWater.Checked ? ((DevComponents.Editors.ComboItem)cbxWater.SelectedItem).Text : string.Empty;

            currentConfiguringAccountSetting.FriendWaterLowLimit = ((DevComponents.Editors.ComboItem)cbxFriendWater.SelectedItem).Text;

            currentConfiguringAccountSetting.AutoVermin = ckxAutoVermin.Checked;
            currentConfiguringAccountSetting.AutoPlough = ckxAutoPlough.Checked;

            currentConfiguringAccountSetting.AutoBuySeed = ckxAutoBuySeed.Checked;

            currentConfiguringAccountSetting.AutoHavest = ckxAutoHavest.Checked;
            currentConfiguringAccountSetting.AutoHavestInTime = ckxAutoHavestInTime.Checked;
            currentConfiguringAccountSetting.AutoStealInTime = ckxAutoStealInTime.Checked;

            currentConfiguringAccountSetting.AutoGrass = ckxAutoGrass.Checked;

            currentConfiguringAccountSetting.AutoSell = ckbAutoSell.Checked;

            currentConfiguringAccountSetting.StealCrops = cbxStealCrops.Text;

            currentConfiguringAccountSetting.IsCare = ckxIsCare.Checked;

            if (isNewAccountFlag)
            {
                gardenSetting.AccountSettings.Add(currentConfiguringAccountSetting);
            }

            if (GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存成功");

                if (isNewAccountFlag)
                {
                    lsbAccount.Items.Add(txtNewLoginEmail.Text);
                    isNewAccountFlag = false;
                }
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存失败");
            }
        }

        private void biFriendUp_Click(object sender, EventArgs e)
        {
            int index = dgvFriendList.SelectedCells[0].RowIndex;

            FriendSetting tempFS = currentConfiguringAccountSetting.FriendSettings[index];

            dgvFriendList.Rows.RemoveAt(index);
            dgvFriendList.Rows.Insert(index - 1, tempFS.Name, tempFS.UID, tempFS.Steal, tempFS.Plough, tempFS.Farm, tempFS.Water, tempFS.Vermin, tempFS.Grass);

            currentConfiguringAccountSetting.FriendSettings[index] = currentConfiguringAccountSetting.FriendSettings[index - 1];
            currentConfiguringAccountSetting.FriendSettings[index - 1] = tempFS;
        }

        private void biFriendDown_Click(object sender, EventArgs e)
        {
            int index = dgvFriendList.SelectedCells[0].RowIndex;

            FriendSetting tempFS = currentConfiguringAccountSetting.FriendSettings[index];

            dgvFriendList.Rows.RemoveAt(index);
            dgvFriendList.Rows.Insert(index + 1, tempFS.Name, tempFS.UID, tempFS.Steal, tempFS.Plough, tempFS.Farm, tempFS.Water, tempFS.Vermin, tempFS.Grass);

            currentConfiguringAccountSetting.FriendSettings[index] = currentConfiguringAccountSetting.FriendSettings[index + 1];
            currentConfiguringAccountSetting.FriendSettings[index + 1] = tempFS;
        }
    }
}
