using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Parking;
using SNSHelper_Win_Garden.Entity;
using SNSHelper_Win_Garden.Helper;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain
    {
        private bool isNewAccount = true;
        private AccountSetting ps_currentAccountSetting = new AccountSetting();
        private Thread loadFriendDataThread;

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

            if (ConfigHelper.SaveConfig())
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("保存成功");
                Utility.NetworkDelay = ConfigHelper.GlobalSetting.NetworkDelay;
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

        private void dgvFriendList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 2: // 是否停车
                    ps_currentAccountSetting.FriendSettings[e.RowIndex].AllowedPark = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 3: // 是否贴条
                    ps_currentAccountSetting.FriendSettings[e.RowIndex].AllowedPost = Convert.ToBoolean(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    break;
                case 4: // 优先级
                    ps_currentAccountSetting.FriendSettings[e.RowIndex].ParkPriority = Convert.ToInt32(dgvFriendList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    // Fix Issue 2
                    if (ps_currentAccountSetting.FriendSettings[e.RowIndex].ParkPriority == 0)
                    {
                        ps_currentAccountSetting.FriendSettings[e.RowIndex].ParkPriority = int.MaxValue;
                    }
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

            if (ConfigHelper.SaveConfig())
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
            }
            else
            {
                btiUp.Enabled = true;
            }

            if (lsbAccount.SelectedItems[0].Index == lsbAccount.Items.Count -1)
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
                    dgvFriendList.Rows.Add(ps_currentAccountSetting.FriendSettings[i].NickName, ps_currentAccountSetting.FriendSettings[i].UID, ps_currentAccountSetting.FriendSettings[i].AllowedPark, ps_currentAccountSetting.FriendSettings[i].AllowedPost);
                }
                else
                {
                    dgvFriendList.Rows.Add(ps_currentAccountSetting.FriendSettings[i].NickName, ps_currentAccountSetting.FriendSettings[i].UID, ps_currentAccountSetting.FriendSettings[i].AllowedPark, ps_currentAccountSetting.FriendSettings[i].AllowedPost, ps_currentAccountSetting.FriendSettings[i].ParkPriority.ToString());

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
                if (ps_currentAccountSetting.FriendSettings[i].NickName.Equals(friend.RealName))
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
                    ps_currentAccountSetting.FriendSettings.Add(new FriendSetting(friendList[i].RealName, friendList[i].UId));
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

            if (Utility.Login(txtNewLoginEmail.Text, txtNewLoginPwd.Text))
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

            ParkingHelper helper = new ParkingHelper();

            this.BeginInvoke(snsif, new object[] { "好友数据获取完毕！正在退出..." });
            Utility.Logout();

            dgtBindParkerFriend bpf = new dgtBindParkerFriend(ShowParkerFriendFromNet);
            this.BeginInvoke(bpf, new object[] { helper.ParkerFriendList });
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
