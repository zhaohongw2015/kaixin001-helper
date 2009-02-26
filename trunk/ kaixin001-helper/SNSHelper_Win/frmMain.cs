using System;
using System.Windows.Forms;
using SNSHelper_Win.Helper;
using System.Threading;
using SNSHelper.Kaixin001.Entity;
using System.Collections.Generic;
using SNSHelper.Kaixin001;
using System.IO;
using SNSHelper_Win.Entity;
using SNSHelper.Common;
using System.Net;
using System.Diagnostics;

namespace SNSHelper_Win
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region 窗体事件
        Thread mainThread;
        Thread checkUpdateThread;
        bool forceClose = false;
        bool isStart = false;

        string latestFile = "Kaixin001.Helper.V1.0.Beta1.zip";

        private void frmMain_Load(object sender, EventArgs e)
        {
            ConfigHelper.LoadConfig();
            timer1.Interval = ConfigHelper.GlobalSetting.ParkInterval * 60000;
            ShowConfigData();
            contextMenuBar2.SetContextMenuEx(lsbAccount, buttonItem7);

            spm = new dgtShowParkingMsg(this.ShowParkingMsg);
            snpt = new dgtShowNextParkingTime(ShowNextParkingTime);

            Utility.NetworkDelay = ConfigHelper.GlobalSetting.NetworkDelay;

            isStart = true;
            checkUpdateThread = new Thread(CheckUpdate);
            checkUpdateThread.Start();
        }

        private void btnStartPark_Click(object sender, EventArgs e)
        {
            btnStartPark.Enabled = false;
            btnStopPark.Enabled = true;
            lblParkingRemainingTime.Visible = false;
            lblNextParkingTime.Visible = true;
            txtPartResultBoard.Clear();

            timer1.Enabled = false;
            mainThread = new Thread(DoPark);
            mainThread.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            lblParkingRemainingTime.Visible = false;
            txtPartResultBoard.Clear();

            mainThread = new Thread(DoPark);
            mainThread.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lblParkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 " + lblParkingRemainingTime.Text).AddSeconds(-1).ToString("HH:mm:ss");
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.Hide();
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

        private void btnStopPark_Click(object sender, EventArgs e)
        {
            if (mainThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("请确定要停止停车操作吗？这可能会引发一些未知的错误！", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    mainThread.Abort();
                    mainThread = null;

                }
            }

            btnStartPark.Enabled = true;
            btnStopPark.Enabled = false;
            timer1.Enabled = false;
            lblNextParkingTime.Text = "操作已停止！";
            lblParkingRemainingTime.Visible = false;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (forceClose)
            {
                if (mainThread != null)
                {
                    mainThread.Abort();
                }
                Application.Exit();
                return;
            }

            if (mainThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("系统正在为你停车，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    mainThread.Abort();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btniExit_Click(object sender, EventArgs e)
        {
            if (mainThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("系统正在为你停车，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    mainThread.Abort();
                    this.Close();
                }
            }

            this.Close();
        }

        private void btniAbout_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();
            frm.ShowDialog();
        }

        private void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            isStart = false;
            if (checkUpdateThread != null && checkUpdateThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                checkUpdateThread = new Thread(CheckUpdate);
                checkUpdateThread.Start();
            }
        }

        private void btniHelper_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "http://code.google.com/p/kaixin001-helper/w/list");
        }

        #endregion

        #region 停车相关方法

        private delegate void dgtShowParkingMsg(string msg);
        private delegate void dgtShowNextParkingTime(string text);
        private delegate void dgtParkingCountDown();
        dgtShowParkingMsg spm;
        dgtShowNextParkingTime snpt;
        private AccountSetting currentAccount;

        private void ParkingCountDown()
        {
            timer1.Enabled = true;
            DateTime dt = DateTime.Now.AddMinutes(ConfigHelper.GlobalSetting.ParkInterval);
            lblNextParkingTime.Text = string.Format("距下次停车({0})还有", dt.ToString("yyyy-MM-dd HH:mm:ss"));
            lblParkingRemainingTime.Visible = true;
            lblParkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 00:00:00").AddMinutes(ConfigHelper.GlobalSetting.ParkInterval).ToString("HH:mm:ss");
            timer2.Enabled = true;
        }

        private void ShowConfigData()
        {
            txtParkingInterval.Text = ConfigHelper.GlobalSetting.ParkInterval.ToString();
            txtNetDelay.Text = (ConfigHelper.GlobalSetting.NetworkDelay / 1000).ToString();

            for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
            {
                lsbAccount.Items.Add(new ListViewItem(ConfigHelper.AccountSettings[i].LoginEmail));
            }
        }

        private void ShowNextParkingTime(string loginEmail)
        {
            lblNextParkingTime.Text = string.Format("系统正在为{0}停车...", loginEmail);
        }

        private void ShowParkingMsg(string msg)
        {
            txtPartResultBoard.AppendText(msg);
        }

        private void ShowParkingMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "\r\n";
            }
            else
            {
                msg = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
            }

            this.Invoke(spm, new object[] { msg });
        }

        private void ShowParkingMessage(List<string> msgList)
        {
            string msg = string.Empty;
            for (int i = 0; i < msgList.Count; i++)
            {
                if (string.IsNullOrEmpty(msgList[i]))
                {
                    msg += "\r\n";
                }
                else
                {
                    msg += string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgList[i]);
                }
            }

            this.Invoke(spm, new object[] { msg });
        }

        private void DoPark()
        {
            ConfigHelper.LoadConfig();

            for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
            {
                currentAccount = ConfigHelper.AccountSettings[i];
                this.Invoke(snpt, new object[] { currentAccount.LoginEmail });

                ShowParkingMessage(string.Format("正在加载帐号{0}的配置信息！", currentAccount.LoginEmail));

                if (!currentAccount.IsOperation)
                {
                    ShowParkingMessage(string.Format("根据配置信息，帐号({0})不操作！", currentAccount.LoginEmail));
                    ShowParkingMessage(string.Empty);
                    continue;
                }
                ShowParkingMessage("开始登录...");

                if (Utility.Login(currentAccount.LoginEmail, currentAccount.LoginPwd))
                {
                    ShowParkingMessage("登录成功！正在获取玩家争车位相关数据...");
                }

                ParkingHelper helper = new ParkingHelper();
                ShowParkingMessage("玩家争车位相关数据获取完毕！");

                ShowParkerDetails(helper.Parker, helper.CarList);
                ShowCarDetails(currentAccount.LoginEmail, helper.CarList);
                ShowFriendParkDetails(currentAccount.LoginEmail, helper.ParkerFriendList);

                ShowParkingMessage(string.Empty);
                ShowParkingMessage("开始停车：");

                // 进行停车
                ParkCars(helper);

                ShowParkingMessage(string.Empty);
                ShowParkingMessage("开始贴条：");

                // 进行贴条
                DoPost(helper);

                ShowParkingMessage(string.Empty);
                ShowParkingMessage(string.Format("当前现金变为{0}元，本次停车共盈利{1}元！", cashBeforeParking, cashBeforeParking - Convert.ToDouble(helper.Parker.Cash)));
                cashBeforeParking = 0;

                ShowParkingMessage(string.Empty);
                ShowParkingMessage("开始退出！");
                Utility.Logout();
                ShowParkingMessage("退出成功！");
                ShowParkingMessage(string.Empty);
            }

            ShowParkingMessage("停车操作完成！");

            ShowParkingMessage(string.Empty);
            ShowParkingMessage(parkedFailedMsg);
            parkedFailedMsg = new List<string>();

            RecordLog(txtPartResultBoard.Text);

            dgtParkingCountDown pcd = new dgtParkingCountDown(ParkingCountDown);
            this.Invoke(pcd, new object[] {});
            mainThread = null;
        }

        private void RecordLog(string log)
        {
            string path = string.Format("{0}\\Log", Application.StartupPath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            StreamWriter sw = File.CreateText(string.Format("{0}\\{1}.log", path, DateTime.Now.ToString("争车位： yyyy-MM-dd HH：mm：ss")));

            sw.Write(log);
            sw.Flush();

            sw.Dispose();
        }

        void ShowParkerDetails(ParkerInfo parker, List<CarInfo> carList)
        {
            double totalCarPrice = GetTotalCarPrice(carList);

            ShowParkingMessage(string.Format("{0}({1})，现金：{2}元，车价：{3}元，总价：{4}元！", parker.RealName, parker.UID, parker.Cash, totalCarPrice, Convert.ToDouble(parker.Cash) + totalCarPrice));
        }

        double GetTotalCarPrice(List<CarInfo> carList)
        {
            double totalPrice = 0;
            for (int i = 0; i < carList.Count; i++)
            {
                totalPrice += Convert.ToDouble(carList[i].Price);
            }

            return totalPrice;
        }

        void ShowCarDetails(string loginEmail, List<CarInfo> carList)
        {
            ShowParkingMessage(string.Empty);
            ShowParkingMessage("停车信息：");

            for (int i = 0; i < carList.Count; i++)
            {
                ShowParkingMessage(carList[i].CarName + "，目前" + (string.IsNullOrEmpty(carList[i].ParkRealName) ? "正在找车位..." : "停在 " + carList[i].ParkRealName + " 的私家车位上，收入" + carList[i].ParkProfit + "元"));
            }
        }

        void ShowFriendParkDetails(string loginEmail, List<ParkerFriendInfo> parkerFriendList)
        {
            ShowParkingMessage(string.Empty);
            ShowParkingMessage("好友信息：");

            for (int i = 0; i < parkerFriendList.Count; i++)
            {
                ShowParkingMessage(string.Format("{0}({1}) {2}", parkerFriendList[i].RealName, parkerFriendList[i].UId, parkerFriendList[i].Full.Equals("0") ? "" : "车位满"));
            }
        }

        void DoPost(ParkingHelper helper)
        {
            PostResult result;
            for (int i = 0; i < helper.ParkingList.Count; i++)
            {
                // 该车位上未停车
                if (helper.ParkingList[i].CarId.Equals("0"))
                {
                   ShowParkingMessage(string.Format("#{0}车位上未停车", i + 1));
                    continue;
                }

                // 车位利润不够贴条
                if (!string.IsNullOrEmpty(helper.ParkingList[i].CarProfit) && Convert.ToInt32(helper.ParkingList[i].CarProfit) <= Convert.ToInt32(currentAccount.MinPost))
                {
                    ShowParkingMessage(string.Format("#{0}车位利润不够贴条", i + 1));
                    continue;
                }

                FriendSetting friendSetting = currentAccount.GetFriendSetting(helper.ParkingList[i].CarUId);

                if (friendSetting == null || friendSetting.AllowedPost)
                {
                    result = helper.PostOneCar(helper.ParkingList[i]);

                    if (result != null)
                    {
                        ShowParkingMessage(string.Format("#{0} {1} 的 {2} (收入{3}元)：{4}", i + 1, helper.ParkingList[i].CarRealName, helper.ParkingList[i].CarName, helper.ParkingList[i].CarProfit, result.Error));

                        if (result.ErrNo.Equals("0"))
                        {
                            cashBeforeParking += Convert.ToDouble(result.Cash);
                        }
                    }
                }
                else
                {
                    ShowParkingMessage(string.Format("#{0} {1} 的 {2} (收入{3}元)：不贴条用户", i + 1, helper.ParkingList[i].CarRealName, helper.ParkingList[i].CarName, helper.ParkingList[i].CarProfit));
                }
            }
        }

        #endregion

        #region AutoUpdate

        private void CheckUpdate()
        {
            if (File.Exists(Path.Combine(Path.Combine(Application.StartupPath, "Version"), "LatestVersion.txt")))
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("更新包准备就绪，是否立即更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "autoupdate.exe"));
                    forceClose = true;
                    FormClosingEventHandler fceh = new FormClosingEventHandler(frmMain_FormClosing);
                    this.Invoke(fceh, new object[] {new object(), new FormClosingEventArgs(CloseReason.TaskManagerClosing, false)});
                }
            }
            else
            {
                HttpHelper httpHelper = new HttpHelper();
                httpHelper.NetworkDelay = 1000;
                string autoUpdateHtml = httpHelper.GetHtml("http://code.google.com/p/kaixin001-helper/wiki/AutoUpdate");
                string newFile = ContentHelper.GetMidString(autoUpdateHtml, "string latestFile = &quot;", "&quot;;");

                if (!string.IsNullOrEmpty(newFile))
                {
                    if (newFile.Equals(latestFile))
                    {
                        if (!isStart)
                        {
                            DevComponents.DotNetBar.MessageBoxEx.Show("没有可用更新！", "提示");
                            return;
                        }
                    }
                    else
                    {
                        if (DevComponents.DotNetBar.MessageBoxEx.Show("有可用更新，是否立即更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            DownloadNewFile(newFile);
                        }
                    }
                }
            }
        }

        private void DownloadNewFile(string newFileName)
        {
            string versionFileFolderPath = Path.Combine(Application.StartupPath, "Version");
            if (!Directory.Exists(versionFileFolderPath))
            {
                Directory.CreateDirectory(versionFileFolderPath);
            }

            string newFilePath = Path.Combine(versionFileFolderPath, newFileName);
            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }

            try
            {
                WebClient client = new WebClient();
                client.DownloadFile("http://kaixin001-helper.googlecode.com/files/" + newFileName, newFilePath);

                FileStream fs = File.Create(Path.Combine(versionFileFolderPath, "LatestVersion.txt"));
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(newFileName);
                sw.Flush();

                sw.Dispose();
                fs.Dispose();

                if (DevComponents.DotNetBar.MessageBoxEx.Show("更新包准备就绪，是否立即更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "autoupdate.exe"));
                    forceClose = true;
                    FormClosingEventHandler fceh = new FormClosingEventHandler(frmMain_FormClosing);
                    this.Invoke(fceh, new object[] { new object(), new FormClosingEventArgs(CloseReason.TaskManagerClosing, false) });
                }
            }
            catch (Exception)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("下载可用更新失败！", "提示");
            }
        }

        #endregion
    }
}
