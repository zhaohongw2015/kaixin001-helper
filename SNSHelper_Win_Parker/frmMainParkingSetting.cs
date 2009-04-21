using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Parking;
using SNSHelper_Win_Parker.Entity;
using SNSHelper_Win_Parker.Helper;

namespace SNSHelper_Win_Parker
{
    public partial class frmMain
    {
        private bool isNewAccount = true;
        private AccountSetting ps_currentAccountSetting = new AccountSetting();
        private Thread loadFriendDataThread;
        private Thread loadAllFriendDataThread;

        #region 委托声明

        private delegate void dgtBindParkerFriend(List<ParkerFriendInfo> friendList);
        private delegate void dgtShowNetworkingStatus_InitFriend(string status);
        private delegate void dgtStopGetFriendData();

        #endregion

        #region 控件事件

        private void btnSaveGlobalSetting_Click(object sender, EventArgs e)
        {
            ConfigHelper.GlobalSetting.NetworkDelay = txtNetDelay.Value * 1000;
            ConfigHelper.GlobalSetting.ParkInterval = txtParkingInterval.Value;
            Utility utility = new Utility();
            if (ConfigHelper.SaveConfig())
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("保存成功");
                utility.NetworkDelay = ConfigHelper.GlobalSetting.NetworkDelay;
                parkingTimer.Interval = ConfigHelper.GlobalSetting.ParkInterval * 60000;
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("保存失败");
            }
        }

        private void lsbAccount_DoubleClick(object sender, EventArgs e)
        {
            ShowAccountSetting();
            labelFriendCount.Text = string.Format("{0}总共{1}位好友", this.ps_currentAccountSetting.LoginEmail,dgvFriendList.Rows.Count.ToString());
        }

        private void btnLoadFriend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewLoginEmail.Text.Trim()) || string.IsNullOrEmpty(txtNewLoginPwd.Text.Trim()))
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("请输入完整的帐号信息！");
                return;
            }

            BeginToGetFriendData();
        }

        private void btnLoadAllFriend_Click(object sender, EventArgs e)
        {
            if (this.lsbAccount.Items.Count<=0) 
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("请输入帐号信息！");
                return;
            }

            BeginToGetAllFriendData();
        }

        private void dgvFriendList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            int iAccountIndex = 0;

            if (!isNewAccount)
                iAccountIndex = this.lsbAccount.SelectedItems[0].Index;

            String sCurrentUid = dgvFriendList.Rows[e.RowIndex].Cells[1].Value.ToString();

            int iCurrentIndex = 0;

            for (int i = 0; i < ps_currentAccountSetting.FriendSettings.Count; i++)
            {
                if (ps_currentAccountSetting.FriendSettings[i].UID.Equals(sCurrentUid))
                {
                    iCurrentIndex = i;
                    break;
                }

            }
            switch (e.ColumnIndex)
            {
                case 2: // 是否停车
                    ps_currentAccountSetting.FriendSettings[iCurrentIndex].AllowedPark = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    if (!isNewAccount)
                     ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[iCurrentIndex].AllowedPark = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 3: // 是否贴条
                    ps_currentAccountSetting.FriendSettings[iCurrentIndex].AllowedPost = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    if (!isNewAccount)
                      ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[iCurrentIndex].AllowedPost = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 5: // 优先级
                    ps_currentAccountSetting.FriendSettings[iCurrentIndex].ParkPriority = Convert.ToInt32(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                    // Fix Issue 2
                    if (ps_currentAccountSetting.FriendSettings[iCurrentIndex].ParkPriority == 0)
                    {
                        ps_currentAccountSetting.FriendSettings[iCurrentIndex].ParkPriority = int.MaxValue;
                    }
                    if (!isNewAccount)
                       ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[iCurrentIndex].ParkPriority = ps_currentAccountSetting.FriendSettings[iCurrentIndex].ParkPriority;
                    break;
                default:
                    break;
            }
        }

        private void btnSaveAccount_Click(object sender, EventArgs e)
        {
            ps_currentAccountSetting.LoginEmail = txtNewLoginEmail.Text;
            ps_currentAccountSetting.LoginPwd = txtNewLoginPwd.Text;
            ps_currentAccountSetting.IsOperation = cbxIsOperation.Checked;
            ps_currentAccountSetting.MasterAccount = txtMaster.Text;
            ps_currentAccountSetting.MinPark = txtMinPark.Value;
            ps_currentAccountSetting.MinPost = txtMinPost.Value;

            ps_currentAccountSetting.AdvanceSettings.AutoUpdateFreePark = cbxIsUpdateFree.Checked;
            ps_currentAccountSetting.AdvanceSettings.AutoBuyCar = cbxBuyCar.Checked;
            ps_currentAccountSetting.AdvanceSettings.MaxCarNo = txtMaxCarNo.Value;
            ps_currentAccountSetting.AdvanceSettings.AutoUpdateCar = cbxCarAutoUpdate.Checked;
            ps_currentAccountSetting.AdvanceSettings.AutoUpdateCarType = cbxUpdateType.Text;
            ps_currentAccountSetting.AdvanceSettings.Color = cbxColor.SelectedIndex == -1 ? 0 : cbxColor.SelectedIndex;


            if (checkAllBoxX.Checked && DevComponents.DotNetBar.MessageBoxEx.Show(this, "你确定要把这次的配置运用到所有帐号吗？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
                {

                    ConfigHelper.AccountSettings[i].IsOperation = cbxIsOperation.Checked;
                    ConfigHelper.AccountSettings[i].MasterAccount = txtMaster.Text;
                    ConfigHelper.AccountSettings[i].MinPark = txtMinPark.Value;
                    ConfigHelper.AccountSettings[i].MinPost = txtMinPost.Value;

                    ConfigHelper.AccountSettings[i].AdvanceSettings.AutoUpdateFreePark = cbxIsUpdateFree.Checked;
                    ConfigHelper.AccountSettings[i].AdvanceSettings.AutoBuyCar = cbxBuyCar.Checked;
                    ConfigHelper.AccountSettings[i].AdvanceSettings.MaxCarNo = txtMaxCarNo.Value;
                    ConfigHelper.AccountSettings[i].AdvanceSettings.AutoUpdateCar = cbxCarAutoUpdate.Checked;
                    ConfigHelper.AccountSettings[i].AdvanceSettings.AutoUpdateCarType = cbxUpdateType.Text;
                    ConfigHelper.AccountSettings[i].AdvanceSettings.Color = cbxColor.SelectedIndex == -1 ? 0 : cbxColor.SelectedIndex;
                }

            }
            checkAllBoxX.Checked = false;

            if (ConfigHelper.SaveConfig())
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存成功");

                if (isNewAccount)
                {
                    lsbAccount.Items.Add(txtNewLoginEmail.Text);
                    lsbAccount.Items[lsbAccount.Items.Count - 1].Selected =true;
                    isNewAccount = false;
                }
                labelAccCount.Text = string.Format("总共{0}个账号信息", lsbAccount.Items.Count.ToString());
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("帐号设置保存失败");
            }
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            isNewAccount = true;
            ClearAccountField();
        }

        private void btiLoadAccountSetting_Click(object sender, EventArgs e)
        {
            if (lsbAccount.SelectedItems.Count != 0)
            {
                ShowAccountSetting();
            }
        }

        private void btiDeleteAccount_Click(object sender, EventArgs e)
        {
            if (lsbAccount.SelectedItems.Count != 0)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("请确定要删除这个帐号吗？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    DeleteAccountSetting();
                }
            }
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
                bTop.Enabled = false;
            }
            else
            {
                btiUp.Enabled = true;
                bTop.Enabled = true;
            }

            if (lsbAccount.SelectedItems[0].Index == lsbAccount.Items.Count -1)
            {
                btiDown.Enabled = false;
                bbutton.Enabled = false;
            }
            else
            {
                btiDown.Enabled = true;
                bbutton.Enabled = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void biFriendList_PopupOpen(object sender, DevComponents.DotNetBar.PopupOpenEventArgs e)
        {
            if (dgvFriendList.RowCount <= 0 || dgvFriendList.SelectedCells[0].RowIndex <0)
            {
                e.Cancel=true; 
                return;
            }

            if (!(dgvFriendList.SelectedCells[0].ColumnIndex == 2 || dgvFriendList.SelectedCells[0].ColumnIndex == 3))
            {
                biInvertSelectFullColumn.Enabled = false;
                biSelectFullColumn.Enabled = false;

            }
            else
            {
                biInvertSelectFullColumn.Enabled = true;
                biSelectFullColumn.Enabled = true;

            }

            int index = dgvFriendList.SelectedCells[0].RowIndex;
            if (index == 0)
            {
                biFriendUp.Enabled = false;

            }
            else
            {
                biFriendUp.Enabled = true;

            }

            if (index == dgvFriendList.Rows.Count - 1)
            {
                biFriendDown.Enabled = false;
            }
            else
            {
                biFriendDown.Enabled = true;
 
            }

        }

        private void biSelectFullColumn_Click(object sender, EventArgs e)
        {
            if (dgvFriendList.SelectedCells.Count == 0)
            {
                return;
            }

            int iAccountIndex = 0;

             if (!isNewAccount)
                 iAccountIndex = this.lsbAccount.SelectedItems[0].Index;

            int columnIndex = dgvFriendList.SelectedCells[0].ColumnIndex;

            

            for (int i = 0; i < dgvFriendList.Rows.Count; i++)
            {
                dgvFriendList.Rows[i].Cells[columnIndex].Value = true;

                switch (columnIndex)
                {
                    case 2:
                        ps_currentAccountSetting.FriendSettings[i].AllowedPark = true;
                        if (!isNewAccount)
                          ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[i].AllowedPark = true;
                        break;
                    case 3:
                        ps_currentAccountSetting.FriendSettings[i].AllowedPost = true;
                        if (!isNewAccount)
                          ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[i].AllowedPost = true;
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

            if (!(columnIndex == 2 || columnIndex == 3)) return;

            int iAccountIndex = 0;

            if (!isNewAccount)
                 iAccountIndex = this.lsbAccount.SelectedItems[0].Index;
            int iCurrentInex = 0;
            String sUid = "";

            for (int i = 0; i < dgvFriendList.Rows.Count; i++)
            {
                dgvFriendList.Rows[i].Cells[columnIndex].Value = !Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                sUid = dgvFriendList.Rows[i].Cells[1].Value.ToString();

                for (int j = 0; j < ps_currentAccountSetting.FriendSettings.Count; j++)
                {
                    if (ps_currentAccountSetting.FriendSettings[j].UID.Equals(sUid))
                    {
                        iCurrentInex = j;
                        break;
                    }
                }

                switch (columnIndex)
                {
                    case 2:
                        ps_currentAccountSetting.FriendSettings[iCurrentInex].AllowedPark = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        if (!isNewAccount)
                           ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[iCurrentInex].AllowedPark = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    case 3:
                        ps_currentAccountSetting.FriendSettings[iCurrentInex].AllowedPost = Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        if (!isNewAccount)
                           ConfigHelper.AccountSettings[iAccountIndex].FriendSettings[iCurrentInex].AllowedPost= Convert.ToBoolean(dgvFriendList.Rows[i].Cells[columnIndex].Value);
                        break;
                    default:
                        break;
                }
            }
        }


        private void biFriendUp_Click(object sender, EventArgs e)
        {
            int index = dgvFriendList.SelectedCells[0].RowIndex;

            FriendSetting tempFS = ps_currentAccountSetting.FriendSettings[index];

            dgvFriendList.Rows.RemoveAt(index);
            dgvFriendList.Rows.Insert(index - 1, tempFS.NickName, tempFS.UID, tempFS.AllowedPark, tempFS.AllowedPost, tempFS.Scenemoney.ToString(), tempFS.ParkPriority.ToString());

            //ps_currentAccountSetting.FriendSettings[index] = ps_currentAccountSetting.FriendSettings[index - 1];
            //ps_currentAccountSetting.FriendSettings[index - 1] = tempFS;
        }

        private void biFriendDown_Click(object sender, EventArgs e)
        {
            int index = dgvFriendList.SelectedCells[0].RowIndex;

            FriendSetting tempFS = ps_currentAccountSetting.FriendSettings[index];

            dgvFriendList.Rows.RemoveAt(index);
            dgvFriendList.Rows.Insert(index + 1, tempFS.NickName, tempFS.UID, tempFS.AllowedPark, tempFS.AllowedPost, tempFS.Scenemoney.ToString(), tempFS.ParkPriority.ToString());

            //ps_currentAccountSetting.FriendSettings[index] = ps_currentAccountSetting.FriendSettings[index + 1];
            //ps_currentAccountSetting.FriendSettings[index + 1] = tempFS;
        }        

       
        private void cbxBuyCar_CheckedChanged(object sender, EventArgs e)
        {
            txtMaxCarNo.Enabled = cbxBuyCar.Checked;
            txtMaxCarNo.Value = 0;
        }

        private void cbxCarAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {

            cbxUpdateType.Enabled = cbxCarAutoUpdate.Checked;
        }

        #endregion

        #region 好友列表相关

        bool filter = false;

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            filter = !string.IsNullOrEmpty(txtKey.Text);

            if (!filter)
            {
                ShowFriendSettings(ps_currentAccountSetting.FriendSettings);
            }
            else
            {
                ShowFriendSettings(ps_currentAccountSetting.FriendSettings.FindAll(delegate(FriendSetting fs) { return fs.NickName.Contains(txtKey.Text) || fs.UID.Contains(txtKey.Text); }));
            }
        }

        private void ShowFriendSettings(List<FriendSetting> friendSettings)
        {
            dgvFriendList.Rows.Clear();

            foreach (FriendSetting item in friendSettings)
            {
                dgvFriendList.Rows.Add(item.NickName, item.UID, item.AllowedPark, item.AllowedPost, item.Scenemoney.ToString(), item.ParkPriority.ToString());
            }
        }

        private void bTop_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;
            if (index == 0) return;
            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Insert(0, lvi);

            AccountSetting temp = ConfigHelper.AccountSettings[index];
            ConfigHelper.AccountSettings.RemoveAt(index);
            ConfigHelper.AccountSettings.Insert(0, temp);

            ConfigHelper.SaveConfig();

        }



        private void btiUp_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;

            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Insert(index - 1, lvi);
            lsbAccount.Refresh();

            AccountSetting temp = ConfigHelper.AccountSettings[index];
            ConfigHelper.AccountSettings[index] = ConfigHelper.AccountSettings[index - 1];
            ConfigHelper.AccountSettings[index - 1] = temp;

            ConfigHelper.SaveConfig();
        }

        private void btiDown_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;

            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Insert(index + 1, lvi);

            AccountSetting temp = ConfigHelper.AccountSettings[index];
            ConfigHelper.AccountSettings[index] = ConfigHelper.AccountSettings[index + 1];
            ConfigHelper.AccountSettings[index + 1] = temp;

            ConfigHelper.SaveConfig();

        }

        private void bbutton_Click(object sender, EventArgs e)
        {
            int index = lsbAccount.SelectedItems[0].Index;
            if (index == lsbAccount.Items.Count - 1) return;

            ListViewItem lvi = lsbAccount.Items[index];
            lsbAccount.Items.RemoveAt(index);
            lsbAccount.Items.Add(lvi);

            AccountSetting temp = ConfigHelper.AccountSettings[index];
            ConfigHelper.AccountSettings.RemoveAt(index);
            ConfigHelper.AccountSettings.Add(temp);

            ConfigHelper.SaveConfig();
        }



        #endregion

        #region 其他

        private void ShowAccountSetting()
        {
            isNewAccount = false;
            ps_currentAccountSetting = ConfigHelper.GetAccountByLoginEmail(lsbAccount.SelectedItems[0].Text);
            txtNewLoginEmail.Text = ps_currentAccountSetting.LoginEmail;
            txtNewLoginPwd.Text = ps_currentAccountSetting.LoginPwd;
            txtMaster.Text = ps_currentAccountSetting.MasterAccount;
            cbxIsOperation.Checked = ps_currentAccountSetting.IsOperation;

            ShowCurrentFriendList();

            txtMinPark.Text = ps_currentAccountSetting.MinPark.ToString();
            txtMinPost.Text = ps_currentAccountSetting.MinPost.ToString();

            ShowAdvanceSettings(ps_currentAccountSetting.AdvanceSettings);
        }

        private void ShowAdvanceSettings(AdvanceSettings advanceSettings)
        {
            cbxIsUpdateFree.Checked = advanceSettings.AutoUpdateFreePark;
            cbxBuyCar.Checked = advanceSettings.AutoBuyCar;
            txtMaxCarNo.Enabled = advanceSettings.AutoBuyCar;
            cbxCarAutoUpdate.Checked = advanceSettings.AutoUpdateCar;
            if (advanceSettings.AutoBuyCar)
            {
                txtMaxCarNo.Value = advanceSettings.MaxCarNo;
                cbxColor.SelectedIndex = advanceSettings.Color;
            }
            cbxUpdateType.Enabled = advanceSettings.AutoUpdateCar;
            if (advanceSettings.AutoUpdateCar)
            {
                cbxUpdateType.Text = advanceSettings.AutoUpdateCarType;
            }
        }

        private void DeleteAccountSetting()
        {
            AccountSetting delettingAccountSetting = ConfigHelper.GetAccountByLoginEmail(lsbAccount.SelectedItems[0].Text);

            if (delettingAccountSetting != null)
            {
                ConfigHelper.AccountSettings.Remove(delettingAccountSetting);
                lsbAccount.Items.RemoveAt(lsbAccount.SelectedItems[0].Index);

                if (ConfigHelper.SaveConfig())
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("删除帐号成功");
                    labelAccCount.Text = string.Format("总共{0}个账号信息", lsbAccount.Items.Count.ToString());
                }
                else
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("删除帐号失败");
                }
            }
        }

        #region 加载争车位好友信息

        private void ShowCurrentFriendList()
        {
            dgvFriendList.Rows.Clear();
            for (int i = 0; i < ps_currentAccountSetting.FriendSettings.Count; i++)
            {
                
                
                if (ps_currentAccountSetting.FriendSettings[i].ParkPriority == int.MaxValue)
                {
                    dgvFriendList.Rows.Add(ps_currentAccountSetting.FriendSettings[i].NickName, ps_currentAccountSetting.FriendSettings[i].UID, ps_currentAccountSetting.FriendSettings[i].AllowedPark, ps_currentAccountSetting.FriendSettings[i].AllowedPost, ps_currentAccountSetting.FriendSettings[i].Scenemoney.ToString());
                }
                else
                {
                    dgvFriendList.Rows.Add(ps_currentAccountSetting.FriendSettings[i].NickName, ps_currentAccountSetting.FriendSettings[i].UID, ps_currentAccountSetting.FriendSettings[i].AllowedPark, ps_currentAccountSetting.FriendSettings[i].AllowedPost,ps_currentAccountSetting.FriendSettings[i].Scenemoney.ToString(), ps_currentAccountSetting.FriendSettings[i].ParkPriority.ToString());

                }
            }
        }

        private void BeginToGetFriendData()
        {
            dgvFriendList.ScrollBars = ScrollBars.None;
            groupPanel2.Enabled = false;
            groupPanel5.Visible = true;
            ShowNetworkingStatus_InitFriend("正在登录...");

            loadFriendDataThread = new Thread(ShowIniFriendDetails);
            loadFriendDataThread.Start();
        }


        private void BeginToGetAllFriendData()
        {
            dgvFriendList.ScrollBars = ScrollBars.None;
            groupPanel2.Enabled = false;
            groupPanel5.Visible = true;
            ShowNetworkingStatus_InitFriend("正在登录...");

            loadAllFriendDataThread = new Thread(ShowIniAllFriendDetails);
            loadAllFriendDataThread.Start();
        }

        private void StopGetFriendData()
        {
            dgvFriendList.ScrollBars = ScrollBars.Both;
            groupPanel2.Enabled = true;
            groupPanel5.Visible = false;
        }

        private bool IsIncludedInCurrentFriendSettingList(ParkerFriendInfo friend)
        {
            for (int i = 0; i < ps_currentAccountSetting.FriendSettings.Count; i++)
            {
                if (ps_currentAccountSetting.FriendSettings[i].UID.Equals(friend.UId))
                {
                    return true;
                }
            }

            return false;
        }


        private bool IsIncludedInAccountSettingFriendSettingList(AccountSetting currentAccountSetting, ParkerFriendInfo friend)
        {

            for (int i = 0; i < currentAccountSetting.FriendSettings.Count; i++)
            {
                if (currentAccountSetting.FriendSettings[i].UID.Equals(friend.UId))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIncludedInNetFriendList(FriendSetting friend, List<ParkerFriendInfo> friendList)
        {
            for (int i = 0; i < friendList.Count; i++)
            {
                if (friendList[i].UId.Equals(friend.UID))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowParkerFriendFromNet(List<ParkerFriendInfo> friendList)
        {
            if (isNewAccount)
            {
                ps_currentAccountSetting = new AccountSetting();
                ConfigHelper.AccountSettings.Add(ps_currentAccountSetting);
            }

            for (int i = 0; i < ps_currentAccountSetting.FriendSettings.Count; i++)
            {
                if (!IsIncludedInNetFriendList(ps_currentAccountSetting.FriendSettings[i], friendList))
                {
                    ps_currentAccountSetting.FriendSettings.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < friendList.Count; i++)
            {
                if (!IsIncludedInCurrentFriendSettingList(friendList[i]))
                {
                    ps_currentAccountSetting.FriendSettings.Add(new FriendSetting(friendList[i].RealName, friendList[i].UId,friendList[i].SceneMoney));
                }
            }

            for (int i = 0; i < friendList.Count; i++)//更新最新的停车位的收入
            {
                for (int j = 0; j < ps_currentAccountSetting.FriendSettings.Count; j++)
                {
                    if (ps_currentAccountSetting.FriendSettings[j].UID.Equals(friendList[i].UId))
                    {
                        ps_currentAccountSetting.FriendSettings[j].Scenemoney = friendList[i].SceneMoney;
                        break;
                    }



                }
            }


            //高级车位优先停车
            for (int i = 0; i < ps_currentAccountSetting.FriendSettings.Count; i++)
            {
                if ((Convert.ToInt32(ps_currentAccountSetting.FriendSettings[i].Scenemoney) > 10) && (ps_currentAccountSetting.FriendSettings[i].ParkPriority == int.MaxValue))
                {
                    ps_currentAccountSetting.FriendSettings[i].ParkPriority = 21 - Convert.ToInt32(ps_currentAccountSetting.FriendSettings[i].Scenemoney);
                }
            }


            ShowCurrentFriendList();

            StopGetFriendData();
        }

        private void ShowNetworkingStatus_InitFriend(string status)
        {
            lblNetworkingStatus.Text = status;
        }

        private void ShowIniFriendDetails()
        {
            dgtShowNetworkingStatus_InitFriend snsif = new dgtShowNetworkingStatus_InitFriend(ShowNetworkingStatus_InitFriend);
            if (utility.Login(txtNewLoginEmail.Text, txtNewLoginPwd.Text))
            {
                this.BeginInvoke(snsif, new object[] { "登录成功！正在获取好友数据..." });
            }
            else
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("登录失败！");

                dgtStopGetFriendData dgt = new dgtStopGetFriendData(StopGetFriendData);
                this.Invoke(dgt);

                return;
            }

            ParkingHelper helper = new ParkingHelper(utility);

            this.BeginInvoke(snsif, new object[] { "好友数据获取完毕！正在退出..." });
            utility.Logout();

            dgtBindParkerFriend bpf = new dgtBindParkerFriend(ShowParkerFriendFromNet);
            this.BeginInvoke(bpf, new object[] { helper.ParkerFriendList });
        }


        private void ShowIniAllFriendDetails()
        {
            dgtShowNetworkingStatus_InitFriend snsif = new dgtShowNetworkingStatus_InitFriend(ShowNetworkingStatus_InitFriend);
            for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
            {
                if (utility.Login(ConfigHelper.AccountSettings[i].LoginEmail, ConfigHelper.AccountSettings[i].LoginPwd))
                {
                    this.BeginInvoke(snsif, new object[] { string.Format("{0}登录成功！正在获取好友数据...", ConfigHelper.AccountSettings[i].LoginEmail) });
                }
                else
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show(string.Format("{0}登录失败！", ConfigHelper.AccountSettings[i].LoginEmail));
                    continue;

                }

                ParkingHelper helper = new ParkingHelper(utility);

              
                utility.Logout();

                //更新本地的账号好友信息
                for (int j = 0; j < ConfigHelper.AccountSettings[i].FriendSettings.Count; j++)
                {
                    if (!IsIncludedInNetFriendList(ConfigHelper.AccountSettings[i].FriendSettings[j], helper.ParkerFriendList))
                    {
                        ConfigHelper.AccountSettings[i].FriendSettings.RemoveAt(j);
                        j--;
                    }
                }

                for (int j = 0; j < helper.ParkerFriendList.Count; j++)
                {
                    if (!IsIncludedInAccountSettingFriendSettingList(ConfigHelper.AccountSettings[i], helper.ParkerFriendList[j]))
                    {
                        ConfigHelper.AccountSettings[i].FriendSettings.Add(new FriendSetting(helper.ParkerFriendList[j].RealName, helper.ParkerFriendList[j].UId, helper.ParkerFriendList[j].SceneMoney));
                    }
                }

                for (int j = 0; j < helper.ParkerFriendList.Count; j++)//更新最新的停车位的收入
                {
                    for (int k = 0; k < ConfigHelper.AccountSettings[i].FriendSettings.Count; k++)
                    {
                        if (ConfigHelper.AccountSettings[i].FriendSettings[k].UID.Equals(helper.ParkerFriendList[j].UId))
                        {
                            ConfigHelper.AccountSettings[i].FriendSettings[k].Scenemoney = helper.ParkerFriendList[j].SceneMoney;
                            break;
                        }

                    }
                }

                //更新好友信息中默认的停车优先顺序

                for (int j = 0; j < ConfigHelper.AccountSettings[i].FriendSettings.Count; j++)
                {
                    if ((Convert.ToInt32(ConfigHelper.AccountSettings[i].FriendSettings[j].Scenemoney) > 10) && (ConfigHelper.AccountSettings[i].FriendSettings[j].ParkPriority == int.MaxValue))
                    {
                        ConfigHelper.AccountSettings[i].FriendSettings[j].ParkPriority = 21 - Convert.ToInt32(ConfigHelper.AccountSettings[i].FriendSettings[j].Scenemoney);
                    }
                }

                this.BeginInvoke(snsif, new object[] { string.Format("{0}好友数据获取完毕！正在退出...", ConfigHelper.AccountSettings[i].LoginEmail) });
            }
            this.BeginInvoke(snsif, new object[] { "所有账号好友数据获取完毕！正在退出..." });
            dgtStopGetFriendData dgtstop = new dgtStopGetFriendData(StopGetFriendData);
            this.BeginInvoke(dgtstop);
        }

        private void ClearAccountField()
        {
            txtNewLoginEmail.Clear();
            txtNewLoginPwd.Clear();
            txtMaster.Clear();
            cbxIsOperation.Checked = true;
            dgvFriendList.Rows.Clear();
            txtMinPark.Value = 7200;
            txtMinPost.Value = 150;
        }

        #endregion

        #endregion
    }
}
