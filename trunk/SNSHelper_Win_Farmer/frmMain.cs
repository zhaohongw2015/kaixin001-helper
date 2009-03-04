using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SNSHelper.Common;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Garden;
using SNSHelper_Win_Garden.Entity;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        string currentBuildVersion = "20090304b";
        bool isAutoUpdate = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            contextMenuBar2.SetContextMenuEx(lsbAccount, biAccountList);
            contextMenuBar2.SetContextMenuEx(dgvFriendList, biFriendList);

            BindSetting();

            originalTitle = this.Text;

            isAutoUpdate = true;
            BeginCheckUpdate();
        }

        #region Setting

        private void BindSetting()
        {
            isNewAccount = true;

            ShowSeedData();

            InitGardenSetting();

            gardenSetting = GardenSetting.LoadGardenSetting();

            txtParkingInterval.Value = gardenSetting.GlobalSetting.WorkingInterval;
            txtNetDelay.Value = gardenSetting.GlobalSetting.NetworkDelay / 1000;

            Utility.NetworkDelay = gardenSetting.GlobalSetting.NetworkDelay;

            ShowAccountInList(gardenSetting.AccountSettings);
        }

        /// <summary>
        /// 是否新增帐号
        /// </summary>
        private bool isNewAccount = false;

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            isNewAccount = true;

            InitGardenSetting();
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

        SeedData seedData;
        private void ShowSeedData()
        {
            string xml = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "SeedData.xml"), Encoding.GetEncoding("GB2312"));
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"gb2312\" ?>", "");

            seedData = new SeedData(xml);
            BindSeedCombox(seedData);
        }

        private void BindSeedCombox(SeedData seedData)
        {
            foreach (SeedItem item in seedData.SeedItems)
            {
                cbxCrops.Items.Add(item.Name);
            }
        }

        #endregion

        #region Load friend from net

        Thread loadNetFriendThread;

        private void btnLoadFriend_Click(object sender, EventArgs e)
        {
            if (farmerWorkingThread != null)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("农夫定时任务正在运行，暂时不能进行账户导入工作！", "提示");
                return;
            }
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

            loadNetFriendThread = new Thread(LoadNetFriend);
            loadNetFriendThread.Start();
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

            if (Utility.Login(txtNewLoginEmail.Text, txtNewLoginPwd.Text))
            {
                this.BeginInvoke(showLoadNetFriendStatus, new object[] { "登录成功！正在获取好友数据..." });
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("登录失败！");

                StopGetNetFriendWhileLoadingNetFriend();

                return;
            }

            GardenHelper helper = new GardenHelper();

            Dictionary<string, string> friends = helper.GetGardenFriend();

            this.BeginInvoke(showLoadNetFriendStatus, new object[] { "好友数据获取完毕！正在退出..." });

            Utility.Logout();

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

        private void InitGardenSetting()
        {
            txtNewLoginEmail.Clear();
            txtNewLoginPwd.Clear();

            dgvFriendList.Rows.Clear();

            ckxAutoFarm.Checked = true;
            cbxCrops.Enabled = true;
            cbxCrops.SelectedIndex = 0;

            ckxAutoWater.Checked = true;
            cbxWater.Enabled = true;
            cbxWater.SelectedIndex = 0;
            cbxFriendWater.SelectedIndex = 4;

            ckxAutoVermin.Checked = true;
            ckxAutoPlough.Checked = true;

            ckxAutoBuySeed.Checked = true;

            ckxAutoHavest.Checked = true;

            ckxAutoGrass.Checked = true;
        }

        private void ckxAutoFarm_CheckedChanged(object sender, EventArgs e)
        {
            cbxCrops.Enabled = ckxAutoFarm.Checked;
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

        private void ShowAccountInList(List<SNSHelper_Win_Garden.Entity.AccountSetting> accountSettings)
        {
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

        private void ShowAccountSetting(SNSHelper_Win_Garden.Entity.AccountSetting accountSetting)
        {
            txtNewLoginEmail.Text = accountSetting.LoginEmail;
            txtNewLoginPwd.Text = accountSetting.LoginPassword;
            cbxIsOperation.Checked = accountSetting.IsOperate;

            ShowFriendSettings(accountSetting.FriendSettings);

            ckxAutoFarm.Checked = accountSetting.AutoFarm;
            cbxCrops.Enabled = accountSetting.AutoFarm;
            cbxCrops.Text = accountSetting.Crops;

            ckxAutoWater.Checked = accountSetting.AutoWater;
            cbxWater.Enabled = accountSetting.AutoWater;
            cbxWater.Text = Convert.ToInt32(accountSetting.WaterLowLimit) >= 5 ? "4" : accountSetting.WaterLowLimit;

            cbxFriendWater.Text = Convert.ToInt32(accountSetting.FriendWaterLowLimit) >= 5 ? "4" : accountSetting.FriendWaterLowLimit;

            ckxAutoPlough.Checked = accountSetting.AutoPlough;
            ckxAutoVermin.Checked = accountSetting.AutoVermin;

            ckxAutoVermin.Checked = accountSetting.AutoBuySeed;

            ckxAutoHavest.Checked = accountSetting.AutoHavest;

            ckxAutoGrass.Checked = accountSetting.AutoGrass;

            currentConfiguringAccountSetting = accountSetting;
            isNewAccount = false;
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

            GardenSetting.SaveGardenSetting(gardenSetting);

            lsbAccount.Items.Remove(lsbAccount.SelectedItems[0]);

            DevComponents.DotNetBar.MessageBoxEx.Show("删除帐号成功！", "提示");
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

            GardenSetting.SaveGardenSetting(gardenSetting);
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

            GardenSetting.SaveGardenSetting(gardenSetting);

        }

        private void btnSaveGlobalSetting_Click(object sender, EventArgs e)
        {
            gardenSetting.GlobalSetting.WorkingInterval = txtParkingInterval.Value;
            gardenSetting.GlobalSetting.NetworkDelay = txtNetDelay.Value * 1000;

            GardenSetting.SaveGardenSetting(gardenSetting);

            DevComponents.DotNetBar.MessageBoxEx.Show("保存设置成功！", "提示");

            Utility.NetworkDelay = gardenSetting.GlobalSetting.NetworkDelay;
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
            currentConfiguringAccountSetting.LoginEmail = txtNewLoginEmail.Text;
            currentConfiguringAccountSetting.LoginPassword = txtNewLoginPwd.Text;

            currentConfiguringAccountSetting.IsOperate = cbxIsOperation.Checked;

            currentConfiguringAccountSetting.AutoFarm = ckxAutoFarm.Checked;
            currentConfiguringAccountSetting.Crops = ckxAutoFarm.Checked ? cbxCrops.Text : string.Empty;

            currentConfiguringAccountSetting.AutoWater = ckxAutoWater.Checked;
            currentConfiguringAccountSetting.WaterLowLimit = ckxAutoWater.Checked ? ((DevComponents.Editors.ComboItem)cbxWater.SelectedItem).Text : string.Empty;

            currentConfiguringAccountSetting.FriendWaterLowLimit = ((DevComponents.Editors.ComboItem)cbxFriendWater.SelectedItem).Text;

            currentConfiguringAccountSetting.AutoVermin = ckxAutoVermin.Checked;
            currentConfiguringAccountSetting.AutoPlough = ckxAutoPlough.Checked;

            currentConfiguringAccountSetting.AutoBuySeed = ckxAutoBuySeed.Checked;

            currentConfiguringAccountSetting.AutoHavest = ckxAutoHavest.Checked;

            currentConfiguringAccountSetting.AutoGrass = ckxAutoGrass.Checked;

            if (isNewAccount)
            {
                gardenSetting.AccountSettings.Add(currentConfiguringAccountSetting);
            }

            if (GardenSetting.SaveGardenSetting(gardenSetting))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存成功");

                if (isNewAccount)
                {
                    lsbAccount.Items.Add(txtNewLoginEmail.Text);
                    isNewAccount = false;
                }
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存失败");
            }
        }

        #endregion

        #region 花园农夫主逻辑

        Thread farmerWorkingThread;

        /// <summary>
        /// 显示工作信息
        /// </summary>
        MethodWithParmString showMsgWhileWorking;

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartWork();
        }

        private void StartWork()
        {
            if (farmerWorkingThread == null || farmerWorkingThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                lblNextWorkingTime.Visible = true;
                lblWorkingRemainingTime.Visible = false;

                farmerWorkingThread = new Thread(Working);
                farmerWorkingThread.Start();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopWork();
        }

        private void StopWork()
        {
            if (farmerWorkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("请确定要停止操作吗？这可能会引发一些未知的错误！", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    farmerWorkingThread.Abort();
                    farmerWorkingThread = null;
                }
            }

            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        private void ShowCurrentAccoutFarmerWorkingFor(string email)
        {
            lblNextWorkingTime.Text = string.Format("农夫正在为 {0} 服务", email);
        }

        private void ShowCurrentAccoutFarmerWorkingForWhileWorking(string email)
        {
            MethodWithParmString showCurrentAccoutFarmerWorkingFor = new MethodWithParmString(ShowCurrentAccoutFarmerWorkingFor);
            this.Invoke(showCurrentAccoutFarmerWorkingFor, new object[] { email });
        }

        private void Working()
        {
            showMsgWhileWorking = new MethodWithParmString(ShowMsg);

            foreach (SNSHelper_Win_Garden.Entity.AccountSetting workingAccountSetting in gardenSetting.AccountSettings)
            {
                if (!workingAccountSetting.IsOperate)
                {
                    continue;
                }

                ShowCurrentAccoutFarmerWorkingForWhileWorking(workingAccountSetting.LoginEmail);

                ShowMsgWhileWorking(string.Format("正在读取帐号 {0} 的配置信息！", workingAccountSetting.LoginEmail));
                ShowMsgWhileWorking("正在登录...");

                if (!Utility.Login(workingAccountSetting.LoginEmail, workingAccountSetting.LoginPassword))
                {
                    ShowMsgWhileWorking(string.Format("登录失败！请检查帐号({0})配置的登录信息！", workingAccountSetting.LoginEmail));
                    continue;
                }

                ShowMsgWhileWorking("登录成功！正在进入我的花园...");
                GardenHelper helper = new GardenHelper();
                helper.GotoMyGarden();

                #region 显示花园信息
                ShowMsgWhileWorking("正在读取花园信息...");
                GardenDetails gardenDetails = helper.GetGardenDetails(null);

                if (!string.IsNullOrEmpty(gardenDetails.ErrMsg))
                {
                    ShowMsgWhileWorking("读取花园信息失败！！！");
                    ShowMsgWhileWorking("");

                    continue;
                }

                ShowMsgWhileWorking("农田信息：");
                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    ShowMsgWhileWorking(string.Format("{0}号农田：{1} {2} 水{3} 害虫{4} {6} [{5}]",
                                                        gi.FarmNum,
                                                        string.IsNullOrEmpty(gi.Crops) ? "无农作物" : GetSeedName(gi.SeedId),
                                                        string.IsNullOrEmpty(gi.Crops) ? "" : gi.Crops.Replace("<br>", " ").Replace("<font size='12' color='#666666'>", "").Replace("</font>", ""),
                                                        gi.Water,
                                                        gi.Vermin,
                                                        GetCropStatusDesc(gi.CropsStatus),
                                                        gi.Grass == "1" ? "长草了" : ""));
                }
                #endregion

                #region 显示用户情况
                ShowMsgWhileWorking("");
                ShowMsgWhileWorking(string.Format("{0}，{1}，{2}！", gardenDetails.Account.Name, gardenDetails.Account.RankTip, gardenDetails.Account.CashTip));
                ShowMsgWhileWorking("");
                #endregion

                #region 收获

                if (workingAccountSetting.AutoHavest)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsStatus == "2")
                        {
                            //ShowMsgWhileWorking(string.Format("正在收割{0}号农田上的{1}...", gi.FarmNum, GetSeedName(gi.SeedId)));
                            HavestResult hr = helper.Havest(gi.FarmNum, null, null);

                            if (hr.Ret == "succ")
                            {
                                ShowMsgWhileWorking(string.Format("从{0}号农田上收获{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName));
                                gi.CropsStatus = "3";
                            }
                            else
                            {
                                ShowMsgWhileWorking(hr.Reason);
                            }
                        }
                    }
                }

                #endregion

                #region 浇水

                if (workingAccountSetting.AutoWater)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.Status == "1" && Convert.ToInt32(gi.Water) <= Convert.ToInt32(workingAccountSetting.WaterLowLimit))
                        {
                            //ShowMsgWhileWorking(string.Format("正在给{0}号农田浇水...", gi.FarmNum));

                            if (helper.WaterFarm(gi.FarmNum, null, null))
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，浇水成功！", gi.FarmNum));
                            }
                            else
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，浇水失败！", gi.FarmNum));
                            }
                        }
                    }
                }


                #endregion

                #region 捉虫

                if (workingAccountSetting.AutoVermin)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.Vermin > 0)
                        {
                            //ShowMsgWhileWorking(string.Format("{0}号农田上有{1}条虫子，正在捉虫...", gi.FarmNum, gi.Vermin));

                            if (helper.AntiVermin(gi.FarmNum, null, null))
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，捉到{1}条虫子！", gi.FarmNum, gi.Vermin));
                            }
                            else
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，捉虫失败！！！", gi.FarmNum));
                            }
                        }
                    }
                }

                #endregion

                #region 犁地

                if (workingAccountSetting.AutoPlough)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsStatus == "3")
                        {
                            //ShowMsgWhileWorking(string.Format("正在给{0}号农田犁地...", gi.FarmNum));
                            if (helper.Plough(gi.FarmNum, null, null))
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，犁地成功！", gi.FarmNum));
                                gi.CropsStatus = "";
                                gi.CropsId = "0";
                            }
                            else
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，犁地失败！！！", gi.FarmNum));
                            }
                        }
                    }
                }

                #endregion

                #region 锄草

                if (workingAccountSetting.AutoGrass)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.Grass == "1")
                        {
                            //ShowMsgWhileWorking(string.Format("正在给{0}号农田锄草...", gi.FarmNum));
                            if (helper.AntiGrass(gi.FarmNum, null, null))
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，锄草成功！", gi.FarmNum));
                            }
                            else
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，锄草失败！", gi.FarmNum));
                            }
                        }
                    }
                }

                #endregion

                #region 播种

                if (workingAccountSetting.AutoFarm)
                {
                    List<MySeeds> mySeedsList = new List<MySeeds>();

                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsId == "0" && !gi.Shared && gi.Status == "1")
                        {
                            #region 显示用户拥有的种子信息

                            ShowMsgWhileWorking("");
                            ShowMsgWhileWorking("正在获取用户种子信息...");

                            for (int i = 0; i < int.MaxValue; i++)
                            {
                                MySeeds etMySeeds = helper.GetMySeeds(i + 1);

                                if (etMySeeds.SeedItems.Count > 0)
                                {
                                    mySeedsList.Add(etMySeeds);
                                }

                                if (etMySeeds.TotalPage == 0 || etMySeeds.TotalPage == i + 1)
                                {
                                    break;
                                }
                            }

                            if (mySeedsList.Count == 0)
                            {
                                ShowMsgWhileWorking("用户没有一粒种子！");
                            }
                            else
                            {
                                string seedInfo = "种子信息：";
                                foreach (MySeeds mySeeds in mySeedsList)
                                {
                                    foreach (SeedItem seedItem in mySeeds.SeedItems)
                                    {
                                        seedInfo += string.Format("{0}({1}) ", seedItem.Name, seedItem.Num);
                                    }
                                }

                                ShowMsgWhileWorking(seedInfo);
                            }

                            #endregion

                            break;
                        }
                    }

                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsId == "0" && !gi.Shared && gi.Status == "1")
                        {
                            //ShowMsgWhileWorking(string.Format("{0}号农田可种植农作物...", gi.FarmNum));

                            SeedItem si = GetSeedItemForFarming(helper, mySeedsList, workingAccountSetting.Crops);

                            if (si == null)
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，种植失败！！！", gi.FarmNum));
                            }
                            else
                            {
                                if (helper.FarmSeed(gi.FarmNum, null, si.SeedID))
                                {
                                    ShowMsgWhileWorking(string.Format("{0}号农田，成功种植{1}！", gi.FarmNum, workingAccountSetting.Crops));
                                    si.Num--;
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 显示仓库信息
                ShowMsgWhileWorking("");
                ShowMsgWhileWorking("正在读取仓库信息...");
                Granary myGranary = helper.GetMyGranary();

                string granaryInfo = "仓库信息：";
                foreach (FruitItem fruit in myGranary.FruitItems)
                {
                    granaryInfo += string.Format("{0}({1})，", fruit.Name, fruit.Num);
                }
                granaryInfo += string.Format("总价：{0}元！", myGranary.TotalPrice);
                ShowMsgWhileWorking(granaryInfo);
                #endregion

                ShowMsgWhileWorking("");
                ShowMsgWhileWorking(string.Format("{0} 的花园工作完毕！", workingAccountSetting.LoginEmail));

                #region 去好友花园“做事”

                foreach (FriendSetting friendSetting in workingAccountSetting.FriendSettings)
                {
                    if (friendSetting.Steal || friendSetting.Plough || friendSetting.Water || friendSetting.Vermin || friendSetting.Farm || friendSetting.Grass)
                    {

                        ShowMsgWhileWorking("");
                        ShowMsgWhileWorking(string.Format("农夫正在前往 {0} 的花园...", friendSetting.Name));
                        helper.GotoFriendGarden(friendSetting.UID);

                        #region 读取好友花园信息
                        ShowMsgWhileWorking("正在读取花园信息...");
                        GardenDetails friendGardenDetails = helper.GetGardenDetails(friendSetting.UID);

                        ShowMsgWhileWorking("农田信息：");
                        foreach (GardenItem gi in friendGardenDetails.GarderItems)
                        {
                            ShowMsgWhileWorking(string.Format("{0}号农田：{1} {2} 水{3} 害虫{4} {6} [{5}]",
                                                                gi.FarmNum,
                                                                string.IsNullOrEmpty(gi.Crops) ? "无农作物" : GetSeedName(gi.SeedId),
                                                                string.IsNullOrEmpty(gi.Crops) ? "" : gi.Crops.Replace("<br>", " ").Replace("<font size='12' color='#666666'>", "").Replace("</font>", "").Replace("<font color='#FF0000'>", "").Replace("<font>", ""),
                                                                gi.Water,
                                                                gi.Vermin,
                                                                GetCropStatusDesc(gi.CropsStatus),
                                                                gi.Grass == "1" ? "长草了" : ""));
                        }
                        #endregion

                        #region 偷果实

                        if (friendSetting.Steal)
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.CropsStatus == "2" && !gi.Crops.Contains("已偷过"))
                                {
                                    //ShowMsgWhileWorking(string.Format("正在偷取好友{0}号农田上的{1}...", gi.FarmNum, GetSeedName(gi.SeedId)));
                                    HavestResult hr = helper.Havest(gi.FarmNum, friendSetting.UID, null);

                                    if (hr.Ret == "succ")
                                    {
                                        ShowMsgWhileWorking(string.Format("从好友的{0}号农田上偷取{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName));
                                        gi.CropsStatus = "3";
                                    }
                                    else
                                    {
                                        ShowMsgWhileWorking(hr.Reason);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region 浇水

                        if (friendSetting.Water)
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.Status == "1" && Convert.ToInt32(gi.Water) <= Convert.ToInt32(workingAccountSetting.FriendWaterLowLimit))
                                {
                                    //ShowMsgWhileWorking(string.Format("正在给好友的{0}号农田浇水...", gi.FarmNum));

                                    if (helper.WaterFarm(gi.FarmNum, friendSetting.UID, null))
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，浇水成功！", gi.FarmNum));
                                    }
                                    else
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，浇水失败！", gi.FarmNum));
                                    }
                                }
                            }

                        }

                        #endregion

                        #region 捉虫

                        if (friendSetting.Vermin)
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.Vermin > 0)
                                {
                                    //ShowMsgWhileWorking(string.Format("好友{0}号农田上有{1}条虫子，正在捉虫...", gi.FarmNum, gi.Vermin));

                                    if (helper.AntiVermin(gi.FarmNum, friendSetting.UID, null))
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，捉到{1}条虫子！", gi.FarmNum, gi.Vermin));
                                    }
                                    else
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，捉虫失败！！！", gi.FarmNum));
                                    }
                                }
                            }
                        }

                        #endregion

                        #region 播种

                        #endregion

                        #region 锄草

                        if (friendSetting.Grass)
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.Grass == "1")
                                {
                                    //ShowMsgWhileWorking(string.Format("好友{0}号农田上长草了，正在锄草...", gi.FarmNum));

                                    if (helper.AntiGrass(gi.FarmNum, friendSetting.UID, null))
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，锄草成功！", gi.FarmNum));
                                    }
                                    else
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，锄草失败！", gi.FarmNum));
                                    }
                                }
                            }

                        }

                        #endregion
                    }
                }

                #endregion

                ShowMsgWhileWorking("");
                ShowMsgWhileWorking("正在退出...");
                Utility.Logout();
                ShowMsgWhileWorking("退出成功！");
                ShowMsgWhileWorking("");
            }

            StartCountDownWhileWorking();
        }

        private void StartCountDownWhileWorking()
        {
            MethodWithoutParm startCountDownWhileWorking = new MethodWithoutParm(StartCountDown);

            this.Invoke(startCountDownWhileWorking);
        }

        private void StartCountDown()
        {
            farmerWorkingThread = null;

            // 显示下次工作时刻
            DateTime dt = DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval);
            lblNextWorkingTime.Text = string.Format("距下次工作({0})还有", dt.ToString("yyyy-MM-dd HH:mm:ss"));

            // 显示距下次工作时刻的时间差
            lblWorkingRemainingTime.Visible = true;
            lblWorkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 00:00:00").AddMinutes(gardenSetting.GlobalSetting.WorkingInterval).ToString("HH:mm:ss");

            countDownTimer.Enabled = true;
        }

        private string GetCropStatusDesc(string cropsStatusNum)
        {
            switch (cropsStatusNum)
            {
                case "1":
                    return "生长中";
                case "2":
                    return "已成熟";
                case "3":
                    return "已收获";
                default:
                    return string.Empty;
            }
        }

        private SeedItem GetSeedItem(List<MySeeds> mySeedsList, string seedName)
        {
            foreach (MySeeds mySeedsItem in mySeedsList)
            {
                foreach (SeedItem seedItem in mySeedsItem.SeedItems)
                {
                    if (seedItem.Name.Equals(seedName))
                    {
                        if (seedItem.Num > 0)
                        {
                            return seedItem;
                        }
                    }
                }
            }

            return null;
        }

        private void AddSeedItemToMySeedsList(SeedItem si, List<MySeeds> mySeedsList)
        {
            if (mySeedsList.Count == 0)
            {
                mySeedsList.Add(new MySeeds());
            }

            mySeedsList[0].SeedItems.Add(si);
        }

        private SeedItem GetSeedItemForFarming(GardenHelper helper, List<MySeeds> mySeedsList, string seedName)
        {
            SeedItem seedItem = GetSeedItem(mySeedsList, seedName);
            if (seedItem == null)
            {
                ShowMsgWhileWorking(string.Format("没{0}种子了，去商店购买...", seedName));

                string seedId = GetSeedID(seedName);
                if (helper.BuySeed(5, seedId))
                {
                    ShowMsgWhileWorking(string.Format("成功购买5个{0}种子！", seedName));
                    SeedItem si = new SeedItem();
                    si.Name = seedName;
                    si.SeedID = seedId;
                    si.Num = 5;

                    AddSeedItemToMySeedsList(si, mySeedsList);

                    seedItem = si;
                }
                else
                {
                    ShowMsgWhileWorking(string.Format("购买{0}种子失败！", seedName));
                    return null;
                }
            }

            return seedItem;
        }

        #region Show message

        private void ShowMsg(string msg)
        {
            txtWorkingBoard.AppendText(msg);
        }

        private void ShowMsgWhileWorking(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "\r\n";
            }
            else
            {
                msg = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
            }

            this.Invoke(showMsgWhileWorking, new object[] { msg });
        }

        #endregion

        #region Check new version & download new file

        Thread updateThread;
        private void buttonItem5_Click(object sender, EventArgs e)
        {
            isAutoUpdate = false;
            BeginCheckUpdate();
        }

        private void BeginCheckUpdate()
        {
            if (updateThread == null)
            {
                updateThread = new Thread(CheckUpdate);
                updateThread.Start();
            }
        }

        int newFileSize;
        private void CheckUpdate()
        {
            ChangeFormTitleInThread(originalTitle + "   正在检查更新...");
            HttpHelper httpHelper = new HttpHelper();
            httpHelper.NetworkDelay = 1000;
            string autoUpdateHtml = httpHelper.GetHtml("http://code.google.com/p/kaixin001-helper/wiki/Farmer");

            if (string.IsNullOrEmpty(autoUpdateHtml))
            {
                if (!isAutoUpdate)
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("检查更新失败，请稍候重试！", "提示");
                }

                ChangeFormTitleInThread(originalTitle + "   检查更新失败，请稍候重试！");

                updateThread = null;
                return;
            }

            string latestBuildVersion = ContentHelper.GetMidString(autoUpdateHtml, "string last = &quot;", "&quot;;");
            string filePath = ContentHelper.GetMidString(autoUpdateHtml, "string file = &quot;", "&quot;;");
            newFileSize = Convert.ToInt32(ContentHelper.GetMidString(autoUpdateHtml, "string size = &quot;", "&quot;;"));

            if (latestBuildVersion != currentBuildVersion)
            {
                NoticeUserForUpdateWhileCheckingUpdate(filePath);
            }
            else
            {
                if (!isAutoUpdate)
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("未发现新版本", "提示");
                }

                ChangeFormTitleInThread(originalTitle);
                updateThread = null;
            }
        }

        private void NoticeUserForUpdateWhileCheckingUpdate(string filePath)
        {
            MethodWithParmString noticeUserForUpdateWhileCheckingUpdate = new MethodWithParmString(NoticeUserForUpdate);
            this.Invoke(noticeUserForUpdateWhileCheckingUpdate, new object[] { filePath });
        }

        private void NoticeUserForUpdate(string filePath)
        {
            if (DevComponents.DotNetBar.MessageBoxEx.Show(this, "发现开心网花园农夫新版本，是否立即下载？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                BeginDownLoadFile(filePath);
            }
            else
            {
                ChangeFormTitleInThread(originalTitle + "   发现新版本，尚未下载...");
                updateThread = null;
            }
        }

        string originalTitle;
        Thread downloadFileThread;
        private void BeginDownLoadFile(string filePath)
        {
            if (downloadFileThread == null)
            {
                downloadFileThread = new Thread(DownLoadFile);
                downloadFileThread.Start(filePath);
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "正在下载新版本，请稍候！", "提示");
            }
        }

        private void DownLoadFile(object file)
        {
            try
            {
                string filePath = file.ToString();
                ChangeFormTitleInThread(originalTitle + "   正在下载新版本...");

                filePath = "http://" + filePath;

                string[] items = filePath.Split('/');
                string localFile = Path.Combine(Path.Combine(Application.StartupPath, "New"), items[items.Length - 1]);

                if (File.Exists(localFile) && new FileInfo(localFile).Length == newFileSize)
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("新版本以下载至 " + localFile, "提示");

                    ChangeFormTitleInThread(originalTitle + "   新版本下载完毕...");

                    return;
                }

                if (!Directory.Exists(Path.Combine(Application.StartupPath, "New")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "New"));
                }

                WebClient client = new WebClient();
                client.DownloadFile(filePath, localFile);

                ChangeFormTitleInThread(originalTitle + "   新版本下载完毕...");

                ShowAlertInThread("新版本以下载至 " + localFile);
            }
            catch (Exception)
            {
                ShowAlertInThread("新版本下载失败");
            }
            finally
            {
                updateThread = null;
            }
        }

        private void ShowAlertInThread(string msg)
        {
            MethodWithParmString showAlertInThread = new MethodWithParmString(ShowAlert);
            this.Invoke(showAlertInThread, new object[] { msg });
        }

        private void ShowAlert(string msg)
        {
            DevComponents.DotNetBar.MessageBoxEx.Show(this, msg, "提示");
        }

        private void ChageFormTitle(string title)
        {
            this.Text = title;
        }

        private void ChangeFormTitleInThread(string title)
        {
            MethodWithParmString changeFormTitleInThread = new MethodWithParmString(ChageFormTitle);
            this.Invoke(changeFormTitleInThread, new object[] { title });
        }

        #endregion

        #endregion

        private void buttonItem7_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "http://code.google.com/p/kaixin001-helper/w/list");
        }

        private void buttonItem4_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "http://code.google.com/p/kaixin001-helper/w/list");
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            frmAbout newfrm = new frmAbout();
            newfrm.ShowDialog();
        }

        private void countDownTimer_Tick(object sender, EventArgs e)
        {
            lblWorkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 " + lblWorkingRemainingTime.Text).AddSeconds(-1).ToString("HH:mm:ss");

            if (lblWorkingRemainingTime.Text == "00:00:00")
            {
                countDownTimer.Enabled = false;
                StartWork();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (farmerWorkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("农夫正在为你工作，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    farmerWorkingThread.Abort();
                }
                else
                {
                    e.Cancel = true;
                }
            }

        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.Hide();
            }
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            if (farmerWorkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("农夫正在为你工作，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    farmerWorkingThread.Abort();
                    farmerWorkingThread = null;
                    this.Close();
                }
            }
        }

        private void contextMenuBar2_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0 || dgvFriendList.SelectedCells[0].ColumnIndex == 0)
            {
                e.Cancel = true;

                return;
            }
        }

        private void dgvFriendList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point point = dgvFriendList.PointToClient(MousePosition);

                System.Windows.Forms.DataGridView.HitTestInfo hitInfo = dgvFriendList.HitTest(point.X, point.Y);
                if (hitInfo.RowIndex != -1 && hitInfo.ColumnIndex != -1)
                {
                    if (!dgvFriendList.Rows[hitInfo.RowIndex].Cells[hitInfo.ColumnIndex].Selected)
                    {
                        dgvFriendList.ClearSelection();
                        dgvFriendList.Rows[hitInfo.RowIndex].Cells[hitInfo.ColumnIndex].Selected = true;
                    }
                }
            }
        }

        private void biSelectFullRow_Click(object sender, EventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0)
            {
                return;
            }

            int rowIndex = dgvFriendList.SelectedCells[0].RowIndex;

            for (int i = 2; i < dgvFriendList.Rows[rowIndex].Cells.Count; i++)
            {
                dgvFriendList.Rows[rowIndex].Cells[i].Value = true;
            }

            currentConfiguringAccountSetting.FriendSettings[rowIndex].Farm = true;
            currentConfiguringAccountSetting.FriendSettings[rowIndex].Water = true;
            currentConfiguringAccountSetting.FriendSettings[rowIndex].Plough = true;
            currentConfiguringAccountSetting.FriendSettings[rowIndex].Vermin = true;
            currentConfiguringAccountSetting.FriendSettings[rowIndex].Steal = true;
            currentConfiguringAccountSetting.FriendSettings[rowIndex].Grass = true;
        }

        private void biSelectFullColumn_Click(object sender, EventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0)
            {
                return;
            }

            int columnIndex = dgvFriendList.SelectedCells[0].ColumnIndex;

            for (int i = 0; i < dgvFriendList.Rows.Count; i++)
            {
                dgvFriendList.Rows[i].Cells[columnIndex].Value = true;

                switch (columnIndex)
                {
                    case 2:
                        currentConfiguringAccountSetting.FriendSettings[i].Steal = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 3:
                        currentConfiguringAccountSetting.FriendSettings[i].Plough = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 4:
                        currentConfiguringAccountSetting.FriendSettings[i].Farm = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 5:
                        currentConfiguringAccountSetting.FriendSettings[i].Water = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 6:
                        currentConfiguringAccountSetting.FriendSettings[i].Vermin = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 7:
                        currentConfiguringAccountSetting.FriendSettings[i].Grass = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    default:
                        break;
                }
            }
        }

        private void biInvertSelectFullRow_Click(object sender, EventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0)
            {
                return;
            }

            int rowIndex = dgvFriendList.SelectedCells[0].RowIndex;

            for (int i = 2; i < dgvFriendList.Rows[rowIndex].Cells.Count; i++)
            {
                dgvFriendList.Rows[rowIndex].Cells[i].Value = !Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);

                switch (i)
                {
                    case 2:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Steal = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    case 3:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Plough = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    case 4:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Farm = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    case 5:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Water = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    case 6:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Vermin = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    case 7:
                        currentConfiguringAccountSetting.FriendSettings[rowIndex].Grass = Convert.ToBoolean(dgvFriendList.Rows[rowIndex].Cells[i].Value);
                        break;
                    default:
                        break;
                }
            }
        }

        private void biInvertSelectFullColumn_Click(object sender, EventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0)
            {
                return;
            }

            int columnIndex = dgvFriendList.SelectedCells[0].ColumnIndex;

            for (int i = 0; i < dgvFriendList.Rows.Count; i++)
            {
                dgvFriendList.Rows[i].Cells[columnIndex].Value = !Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);

                switch (columnIndex)
                {
                    case 2:
                        currentConfiguringAccountSetting.FriendSettings[i].Steal = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 3:
                        currentConfiguringAccountSetting.FriendSettings[i].Plough = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 4:
                        currentConfiguringAccountSetting.FriendSettings[i].Farm = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 5:
                        currentConfiguringAccountSetting.FriendSettings[i].Water = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 6:
                        currentConfiguringAccountSetting.FriendSettings[i].Vermin = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 7:
                        currentConfiguringAccountSetting.FriendSettings[i].Grass = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    default:
                        break;
                }
            }
        }

        private void biImport_Click(object sender, EventArgs e)
        {
            if (farmerWorkingThread != null)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "农夫定时任务正在运行，请停止后再执行导入操作！", "提示");

                return;
            }

            frmImport newfrm = new frmImport();
            newfrm.ShowDialog();
        }
    }
}
