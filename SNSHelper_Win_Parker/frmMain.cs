using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using SNSHelper.Common;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Parking;
using SNSHelper_Win_Garden.Entity;
using SNSHelper_Win_Garden.Helper;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {

        /// <summary>
        /// 助手的当前版本
        /// </summary>
        string currentBuildVersion = "20090414内测版";
        
        
        
        public frmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 停车线程
        /// </summary>
        private Thread parkingThread;

        /// <summary>
        /// 检查新版本线程
        /// </summary>
        private Thread checkUpdateThread;

        private bool isStart = false;

        private Utility utility = new Utility();

        #region 窗体其他事件
        private void frmMain_Load(object sender, EventArgs e)
        {

            this.Text = "开心网争车位助手 V1.0 " + currentBuildVersion + " --By Jailu";
            ShowWhatsNew(); 
            LoadConfig();

            contextMenuBar2.SetContextMenuEx(lsbAccount, biAccountList);
            contextMenuBar2.SetContextMenuEx(dgvFriendList, biFriendList);

            _dgtShowParkingMsg = new dgtShowParkingMsg(this.ShowParkingMsg);//显示停车信息
            _dgtShowCurrentParkingAccount = new dgtShowCurrentParkingAccount(ShowCurrentParkingAccount);//显示当前账号
            _dgtdftStaParkinfo = new dftStaParkinfo(parkerSta);//统计账号停车信息
            //BeginCheckNewVersion();
        }


        /// <summary>
        /// 显示更新记录
        /// </summary>
        private void ShowWhatsNew()
        {
            string whatsNewFilePath = Path.Combine(Application.StartupPath, "What's New.txt");

            if (File.Exists(whatsNewFilePath))
            {
                txtWhatsNew.AppendText("\r\n");
                txtWhatsNew.AppendText(File.ReadAllText(whatsNewFilePath));
            }
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        private void LoadConfig()
        {
            CarMarketHelper.LoadCarList();
            ConfigHelper.LoadConfig();
            parkingTimer.Interval = ConfigHelper.GlobalSetting.ParkInterval * 60000;
            utility.NetworkDelay = ConfigHelper.GlobalSetting.NetworkDelay;

            ShowConfigData();
        }

        /// <summary>
        /// 显示配置
        /// </summary>
        private void ShowConfigData()
        {
            txtParkingInterval.Text = ConfigHelper.GlobalSetting.ParkInterval.ToString();
            txtNetDelay.Text = (ConfigHelper.GlobalSetting.NetworkDelay / 1000).ToString();

            for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
            {
                lsbAccount.Items.Add(new ListViewItem(ConfigHelper.AccountSettings[i].LoginEmail));
            }

            labelAccCount.Text = string.Format("总共{0}个账号信息",lsbAccount.Items.Count.ToString());
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.Hide();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isForceClosedByUpdateNewVersion)
            {
                if (parkingThread != null)
                {
                    parkingThread.Abort();
                }

                Application.Exit();
                return;
            }

            if (parkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("系统正在为你停车，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    parkingThread.Abort();
                }
                else
                {
                    e.Cancel = true;
                }
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

        private void btniExit_Click(object sender, EventArgs e)
        {
            if (parkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("系统正在为你停车，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    parkingThread.Abort();
                    this.Close();
                }
            }

            this.Close();
        }

        #endregion

        #region Timer

        private void parkingTimer_Tick(object sender, EventArgs e)
        {
            parkingTimer.Enabled = false;
            lblParkingRemainingTime.Visible = false;
            txtPartResultBoard.Clear();

            parkingThread = new Thread(DoPark);
            parkingThread.Start();
        }

        private void countDown_Tick(object sender, EventArgs e)
        {
            lblParkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 " + lblParkingRemainingTime.Text).AddSeconds(-1).ToString("HH:mm:ss");
        }
 
        #endregion

        #region “帮助”菜单事件
        private void btniAbout_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();
            frm.ShowDialog();
        }

        private void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            isStart = false;
            BeginCheckNewVersion();
        }

        private void btniHelper_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "http://code.google.com/p/kaixin001-helper/w/list");
        }
        #endregion

        #region Parking

        #region 开始停车、停止停车

        private void btnStartPark_Click(object sender, EventArgs e)
        {
            btnStartParking.Enabled = false;
            btnStopParking.Enabled = true;
            lblParkingRemainingTime.Visible = false;
            lblNextParkingTime.Visible = true;
            txtPartResultBoard.Clear();

            parkingTimer.Enabled = false;
            parkingThread = new Thread(DoPark);
            parkingThread.Start();
        }

        private void btnStopPark_Click(object sender, EventArgs e)
        {
            if (parkingThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("请确定要停止停车操作吗？这可能会引发一些未知的错误！", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    parkingThread.Abort();
                    parkingThread = null;
                }
            }

            btnStartParking.Enabled = true;
            btnStopParking.Enabled = false;
            parkingTimer.Enabled = false;
            lblNextParkingTime.Text = "操作已停止！";
            lblParkingRemainingTime.Visible = false;
        }
        
        #endregion

        #region Delegate(用于在停车线程中操作窗体其他控件)

        private delegate void dgtShowParkingMsg(string msg);
        private delegate void dgtShowCurrentParkingAccount();
        private delegate void dgtBeginCountDown();
        private delegate void dftStaParkinfo(ParkerStaInfo parkStaInfo);

        dgtShowParkingMsg _dgtShowParkingMsg;
        dgtShowCurrentParkingAccount _dgtShowCurrentParkingAccount;
        dftStaParkinfo _dgtdftStaParkinfo;

        #endregion

        #region CountDown

        /// <summary>
        /// 开始倒计时
        /// </summary>
        private void BeginCountDown()
        {
            dgtBeginCountDown _dgtBeginCountDown = new dgtBeginCountDown(CountDown);
            this.Invoke(_dgtBeginCountDown, new object[] { });
        }

        private void CountDown()
        {
            // 显示下次停车时刻
            DateTime dt = DateTime.Now.AddMinutes(ConfigHelper.GlobalSetting.ParkInterval);
            lblNextParkingTime.Text = string.Format("距下次停车({0})还有", dt.ToString("yyyy-MM-dd HH:mm:ss"));

            // 显示距下次停车时刻的时间差
            lblParkingRemainingTime.Visible = true;
            lblParkingRemainingTime.Text = Convert.ToDateTime("1900-1-1 00:00:00").AddMinutes(ConfigHelper.GlobalSetting.ParkInterval).ToString("HH:mm:ss");

            // 激活两个定时器
            parkingTimer.Enabled = true;
            countDownTimer.Enabled = true;
        }

        #endregion

        #region Show current parking account

        private void ShowCurrentParkingAccount()
        {
            lblNextParkingTime.Text = string.Format("系统正在为{0}停车...", currentParkingAccount.LoginEmail);
        } 

        private void ShowCurrentParkingAccountWhileParking()
        {
            this.Invoke(_dgtShowCurrentParkingAccount);
        }

        #endregion

        #region Show parking message

        /// <summary>
        /// 显示一条停车信息
        /// </summary>
        /// <param name="msg">停车信息</param>
        private void ShowParkingMsg(string msg)
        {
            if (txtPartResultBoard.Lines.Length > 1000)
            {
                RecordLog(txtPartResultBoard.Text);
                txtPartResultBoard.Clear();
            }
            txtPartResultBoard.AppendText(msg);
        }

        #region Invoked in parking thread

        /// <summary>
        /// 显示一条停车信息
        /// </summary>
        /// <param name="msg">停车信息</param>
        private void ShowParkingMsgWhileParking(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "\r\n";
            }
            else
            {
                msg = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
            }

            this.Invoke(_dgtShowParkingMsg, new object[] { msg });
        }

        /// <summary>
        /// 显示一组停车信息
        /// </summary>
        /// <param name="msgList">停车信息列表</param>
        private void ShowParkingMsgWhileParking(List<string> msgList)
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

            this.Invoke(_dgtShowParkingMsg, new object[] { msg });
        }


        /// <summary>
        ///  统计账号信息
        /// </summary>
        /// <param name="parkStaInfo">一个账号停车结果信息表</param>
        private void StaParkinginfo(ParkerStaInfo parkStaInfo)
        {


            this.Invoke(_dgtdftStaParkinfo, new object[] { parkStaInfo });
        } 

        #endregion

        #endregion

        #region Do park

        /// <summary>
        /// 当前进行停车操作的帐号配置
        /// </summary>
        private AccountSetting currentParkingAccount;

        private void DoPark()
        {
            ConfigHelper.LoadConfig();

            for (int i = 0; i < ConfigHelper.AccountSettings.Count; i++)
            {
                currentParkingAccount = ConfigHelper.AccountSettings[i];
                //账号停车统计信息
                try
                {
                    ParkerStaInfo parkerStaInfo = new ParkerStaInfo();

                    ShowCurrentParkingAccountWhileParking();

                    ShowParkingMsgWhileParking(string.Format("\r\n正在加载帐号{0}的配置信息！", currentParkingAccount.LoginEmail));

                    if (!currentParkingAccount.IsOperation)
                    {
                        ShowParkingMsgWhileParking(string.Format("根据配置信息，帐号({0})不操作！", currentParkingAccount.LoginEmail));

                        continue;
                    }

                    ShowParkingMsgWhileParking("开始登录...");

                    if (utility.Login(currentParkingAccount.LoginEmail, currentParkingAccount.LoginPwd))
                    {
                        ShowParkingMsgWhileParking("登录成功！正在获取玩家争车位相关数据...");
                    }
                    else
                    {
                        ShowParkingMsgWhileParking(string.Format("登录失败！请检查帐号({0})配置的登录信息！", currentParkingAccount.LoginEmail));
                        AddParkingFailedMsg(string.Format("登录失败！请检查帐号({0})配置的登录信息！", currentParkingAccount.LoginEmail));

                        continue;
                    }

                    ParkingHelper helper = new ParkingHelper(utility);
                    ShowParkingMsgWhileParking("玩家争车位相关数据获取完毕！");

                    ShowParkerDetails(helper.Parker, helper.CarList);
                    ShowCarDetails(currentParkingAccount.LoginEmail, helper.CarList);
                    ShowFriendParkDetails(currentParkingAccount.LoginEmail, helper.ParkerFriendList);

                    // 停车操作
                    ParkCars(helper);

                    // 贴条操作
                    DoPost(helper);

                    // 高级操作
                    DoAdvanceOpt(helper, currentCash);

                    // 显示本次停车盈利情况
                    ShowParkingMsgWhileParking(string.Format("当前现金变为{0}元，本次停车共盈利{1}元！", currentCash, currentCash - Convert.ToDouble(helper.Parker.Cash)));


                    //账号停车收益情况
                    AddParkingIncomedMsg(string.Format("帐号({0})({4})：当前现金为{1},本次停车共盈利{2}元！,总共{3}辆车", currentParkingAccount.LoginEmail, currentCash, currentCash - Convert.ToDouble(helper.Parker.Cash), helper.CarList.Count.ToString(), helper.Parker.RealName));

                    double dCarCountPrice = 0;
                    foreach (CarInfo c in helper.CarList)
                    {
                        dCarCountPrice = dCarCountPrice + Convert.ToDouble(c.Price);
                    }
                    
                    //账号停车统计信息
                    parkerStaInfo.RealName = helper.Parker.RealName;
                    parkerStaInfo.UId = helper.Parker.UID;
                    parkerStaInfo.Cashcount = currentCash;
                    parkerStaInfo.Carcount = helper.CarList.Count;
                    parkerStaInfo.CarCountPrice = dCarCountPrice;

                    StaParkinginfo(parkerStaInfo);

                    ShowParkingMsgWhileParking("开始退出！");
                    utility.Logout();
                    ShowParkingMsgWhileParking("退出成功！");
                }
                catch (Exception e)
                {
                    
                    AddParkingFailedMsg(string.Format("Exception:帐号({0})停车失败！，错误信息：{1}", currentParkingAccount.LoginEmail,e.Message));
                    continue;
                }
            }

            ShowParkingMsgWhileParking("停车操作完成！");
            
            //显示汇总信息
            ShowParkingMsgWhileParking(parkedIncomeMsg);

            //显示停车失败信息
            ShowParkingMsgWhileParking(parkedFailedMsg);
            ShowParkingMsgWhileParking(string.Format("停车失败总共{0}个账号！", parkedFailedMsg.Count.ToString()));
            
            parkedIncomeMsg.Clear();
            parkedFailedMsg.Clear();

            RecordLog(txtPartResultBoard.Text);

            BeginCountDown();

           

            parkingThread = null;
        }

        #endregion

        #region 高级操作

        private void DoAdvanceOpt(ParkingHelper helper, double currentCash)
        {
            ShowParkingMsgWhileParking("开始执行高级操作：");

            if (currentParkingAccount.AdvanceSettings.AutoUpdateFreePark)
            {
                UpdateFreePark(helper, ref currentCash);
            }
            else
            {
                ShowParkingMsgWhileParking("无需升级免费车位！");
            }

            if (currentParkingAccount.AdvanceSettings.AutoBuyCar)
            {
                AutoBuyCar(helper, ref currentCash);
            }
            else
            {
                ShowParkingMsgWhileParking("无需自动买车！");
            }

            ShowParkingMsgWhileParking("高级操作完成！");
        }

        private void UpdateFreePark(ParkingHelper helper, ref double currentCash)
        {
            for (int i = 0; i < helper.ParkingList.Count; i++)
            {
                if (helper.ParkingList[i].IsParkFree)
                {
                    if (helper.IsCardExsit("15"))
                    {
                        ShowParkingMsgWhileParking("我的道具中已存在车位变更卡，可直接使用！");
                    }
                    else
                    {
                        ShowParkingMsgWhileParking("我的道具中不存在车位变更卡，需先购买！");

                        if (currentCash < 30000)
                        {
                            ShowParkingMsgWhileParking(string.Format("升级免费车位需花费30000，当期现金({0})不足！", currentCash));
                            return;
                        }

                        if (helper.BuyCard("15"))
                        {
                            currentCash -= 30000;
                            ShowParkingMsgWhileParking("购买车位变更卡成功！");
                        }
                        else
                        {
                            ShowParkingMsgWhileParking("购买车位变更卡失败！");
                            return;
                        }
                    }

                    if (helper.UseCard("15"))
                    {
                        ShowParkingMsgWhileParking("升级免费车位成功！");
                        return;
                    }
                    else
                    {
                        ShowParkingMsgWhileParking("升级免费车位失败！");
                        return;
                    }
                }
            }

            ShowParkingMsgWhileParking("不存在免费车位，无需升级！");
        }

        private void AutoBuyCar(ParkingHelper helper, ref double currentCash)
        {
            int currentCarCount = helper.CarList.Count;
            if (currentParkingAccount.AdvanceSettings.MaxCarNo <= currentCarCount)
            {
                ShowParkingMsgWhileParking("您拥有的车辆数以大于或等于配置的最大车辆数，无需自动买车！");
            }
            else
            {
                bool isBuyedNewCar = false;
                for (int i = 0; i < CarMarketHelper.CarList.Count; i++)
                {
                    if (currentCash < CarMarketHelper.CarList[i].Price || currentParkingAccount.AdvanceSettings.MaxCarNo <= currentCarCount)
                    {
                        break;
                    }

                    if (CarMarketHelper.CarList[i].Price <= 70000)
                    {
                        if (IsCarExist(helper.CarList, CarMarketHelper.CarList[i].Name))
                        {
                            continue;
                        }
                    }

                    ShowParkingMsgWhileParking(string.Format("系统正在为你购买新车：{0}！", CarMarketHelper.CarList[i].Name));

                    SNSHelper.Kaixin001.Enum.CarColor color = SNSHelper.Kaixin001.Enum.CarColor.None;
                    if (currentParkingAccount.AdvanceSettings.Color == 0)
                    {
                        Random ran = new Random(DateTime.Now.Millisecond);
                        color = (SNSHelper.Kaixin001.Enum.CarColor)ran.Next(1, 7);
                    }
                    else
                    {
                        color = (SNSHelper.Kaixin001.Enum.CarColor)currentParkingAccount.AdvanceSettings.Color;
                    }

                    if (helper.BuyCar(CarMarketHelper.CarList[i].ID, color))
                    {
                        isBuyedNewCar = true;
                        currentCarCount++;
                        ShowParkingMsgWhileParking(string.Format("购买新车 {0} 成功！", CarMarketHelper.CarList[i].Name));
                        currentCash -= CarMarketHelper.CarList[i].Price;
                    }
                }

                if (isBuyedNewCar)
                {
                    ShowParkingMsgWhileParking("购买新车操作完成！");
                    ShowParkingMsgWhileParking("");
                    ShowParkingMsgWhileParking("系统正在为您的新车停车：");
                    //helper = new ParkingHelper(utility);
                   // ParkCars(helper);
                    ShowParkingMsgWhileParking("");
                }
            }
        }

        private bool IsCarExist(List<CarInfo> carInfoList, string carName)
        {
            for (int i = 0; i < carInfoList.Count; i++)
            {
                if (carInfoList[i].CarName.Equals(carName))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 日志


        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log"></param>
        private void RecordLog(string log)
        {
            string path = string.Format("{0}\\Log", Application.StartupPath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //删除前一天的log文件
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                foreach (FileInfo fl in dir.GetFiles())
                {
                    if (fl.CreationTime < DateTime.Now.AddDays(-1))
                    {
                        try
                        {
                            fl.Delete();
                        }
                        catch {
                            continue;
                        }
                    }
                }
            }

            StreamWriter sw = File.CreateText(string.Format("{0}\\{1}.log", path, DateTime.Now.ToString("争车位： yyyy-MM-dd HH：mm：ss")));

            sw.Write(log);
            sw.Flush();
            sw.Dispose();
        }


        #endregion

        #region 显示帐号争车位相关信息

        private void ShowParkerDetails(ParkerInfo parker, List<CarInfo> carList)
        {
            double totalCarPrice = GetTotalCarPrice(carList);

            ShowParkingMsgWhileParking(string.Format("{0}({1})，现金：{2}元，车价：{3}元，总价：{4}元！", parker.RealName, parker.UID, parker.Cash, totalCarPrice, Convert.ToDouble(parker.Cash) + totalCarPrice));
        }

        private double GetTotalCarPrice(List<CarInfo> carList)
        {
            double totalPrice = 0;
            for (int i = 0; i < carList.Count; i++)
            {
                totalPrice += Convert.ToDouble(carList[i].Price);
            }

            return totalPrice;
        }

        private void ShowCarDetails(string loginEmail, List<CarInfo> carList)
        {
            ShowParkingMsgWhileParking(string.Empty);
            ShowParkingMsgWhileParking("停车信息：");

            for (int i = 0; i < carList.Count; i++)
            {
                ShowParkingMsgWhileParking(carList[i].CarName + "，目前" + (string.IsNullOrEmpty(carList[i].ParkRealName) ? "正在找车位..." : "停在 " + carList[i].ParkRealName + " 的私家车位上，收入" + carList[i].ParkProfit + "元"));
            }
        }

        private void ShowFriendParkDetails(string loginEmail, List<ParkerFriendInfo> parkerFriendList)
        {
            ShowParkingMsgWhileParking(string.Empty);
            ShowParkingMsgWhileParking("好友信息：");

            for (int i = 0; i < parkerFriendList.Count; i++)
            {
                ShowParkingMsgWhileParking(string.Format("{0}({1}) {2}", parkerFriendList[i].RealName, parkerFriendList[i].UId, parkerFriendList[i].Full.Equals("0") ? "" : "车位满"));
            }
        } 

        #endregion

        #region 贴条操作

        /// <summary>
        /// 贴条操作
        /// </summary>
        /// <param name="helper"></param>
        private void DoPost(ParkingHelper helper)
        {
            ShowParkingMsgWhileParking("开始贴条：");

            PostResult result;
            for (int i = 0; i < helper.ParkingList.Count; i++)
            {
                // 该车位上未停车
                if (helper.ParkingList[i].CarId.Equals("0"))
                {
                    ShowParkingMsgWhileParking(string.Format("#{0}车位上未停车", i + 1));
                    continue;
                }

                // 车位利润不够贴条
                if (!string.IsNullOrEmpty(helper.ParkingList[i].CarProfit) && Convert.ToInt32(helper.ParkingList[i].CarProfit) <= Convert.ToInt32(currentParkingAccount.MinPost))
                {
                    ShowParkingMsgWhileParking(string.Format("#{0} {1} 的 {2} (收入{3}元):车位利润不够贴条", i + 1, helper.ParkingList[i].CarRealName, helper.ParkingList[i].CarName, helper.ParkingList[i].CarProfit));
                    continue;
                }

                // 获取停在该车位的车主信息
                FriendSetting friendSetting = currentParkingAccount.GetFriendSetting(helper.ParkingList[i].CarUId);

                if (friendSetting == null || friendSetting.AllowedPost)
                {
                    result = helper.PostOneCar(helper.ParkingList[i]);

                    if (result != null)
                    {
                        ShowParkingMsgWhileParking(string.Format("#{0} {1} 的 {2} (收入{3}元)：{4}", i + 1, helper.ParkingList[i].CarRealName, helper.ParkingList[i].CarName, helper.ParkingList[i].CarProfit, result.Error));

                        if (result.ErrNo.Equals("0"))
                        {
                            currentCash += Convert.ToDouble(result.Cash);
                        }
                    }
                }
                else
                {
                    ShowParkingMsgWhileParking(string.Format("#{0} {1} 的 {2} (收入{3}元)：不贴条用户", i + 1, helper.ParkingList[i].CarRealName, helper.ParkingList[i].CarName, helper.ParkingList[i].CarProfit));
                }
            }
        }

        #endregion
        
        #endregion

        #region AutoUpdate

        string latestFile = "Kaixin001.Helper.V1.0.Beta1.patch001.zip";
        bool isForceClosedByUpdateNewVersion = false;

        private void BeginCheckNewVersion()
        {
            isStart = true;

            if (checkUpdateThread != null && checkUpdateThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                checkUpdateThread = new Thread(CheckNewVersion);
                checkUpdateThread.Start();
            }
        }

        private void CheckNewVersion()
        {
            if (File.Exists(Path.Combine(Path.Combine(Application.StartupPath, "Version"), "LatestVersion.txt")))
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("更新包准备就绪，是否立即更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    UpdateNewVersion();
                }
            }
            else
            {
                HttpHelper httpHelper = new HttpHelper();
                httpHelper.NetworkDelay = 1000;
                string autoUpdateHtml = httpHelper.GetHtml("http://code.google.com/p/kaixin001-helper/wiki/AutoUpdate");
                string latestFileName = ContentHelper.GetMidString(autoUpdateHtml, "string latestFile = &quot;", "&quot;;");

                if (!string.IsNullOrEmpty(latestFileName))
                {
                    if (latestFileName.Equals(latestFile))
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
                            DownloadNewFile(latestFileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 版本升级
        /// </summary>
        private void UpdateNewVersion()
        {
            Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "autoupdate.exe"));
            isForceClosedByUpdateNewVersion = true;
            FormClosingEventHandler fceh = new FormClosingEventHandler(frmMain_FormClosing);
            this.Invoke(fceh, new object[] { new object(), new FormClosingEventArgs(CloseReason.TaskManagerClosing, false) });
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
                    isForceClosedByUpdateNewVersion = true;
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

        private void 官方交流论坛会员限量火热抢注中_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "http://bbs.jailu.cn");
        }

        private void dgvFriendList_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            return;
        }

                
    }
}
