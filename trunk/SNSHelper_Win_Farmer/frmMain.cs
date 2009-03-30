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
using SNSHelper_Win_Garden.Helper;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        /// <summary>
        /// 农夫的当前版本
        /// </summary>
        string currentBuildVersion = "20090330";

        /// <summary>
        /// 标记是否自动检查更新正在运行
        /// </summary>
        bool isAutoUpdateFlag = false;

        #region Thread

        /// <summary>
        /// 下载文件进程
        /// </summary>
        Thread downloadFileThread;

        /// <summary>
        /// 检查更新进程
        /// </summary>
        Thread checkUpdateThread;

        /// <summary>
        /// 农夫工作主线程
        /// </summary>
        Thread farmerWorkingThread;

        /// <summary>
        /// 从网络上加载好友信息线程
        /// </summary>
        Thread getNetFriendThread;

        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            contextMenuBar2.SetContextMenuEx(lsbAccount, biAccountList);
            contextMenuBar2.SetContextMenuEx(dgvFriendList, biFriendList);

            InitSetting();

            originalTitle = this.Text;

            isAutoUpdateFlag = true;
            BeginCheckUpdate();

            ShowWhatsNew();

            showInTimeMsgInThread = new MethodWithParmString(ShowInTimeMsg);
            showInTimeNoticeInThread = new MethodWithParmString(ShowInTimeNotice);

            ShowContributory();

            updateSummaryInThread = new MethodWithObject(UpdateSummary);

            AddSpecialSeed();
            AddSpecialCropsIncome();
        }

        private void ShowContributory()
        {
            Dictionary<string, string> contributoryList = new Dictionary<string, string>();
            contributoryList.Add("乐乐", "Jailu的女朋友。没有乐乐的支持和理解，就没有今天的农夫，鲜花和掌声都献给她吧！");
            contributoryList.Add("高兴网", "http://www.gaoxinga.com: 花园农夫专属版块提供者，让农夫们有了交流的空间！");
            contributoryList.Add("小冲", "无私提供超级群，让农夫们拥有良好的即使交流平台！");
            contributoryList.Add("Bluejacky", "为了让农夫有更好的测试环境，他特意、专门申请了系列帐号！");
            contributoryList.Add("无名有姓", "老前辈，给农夫的开发提供了不少建设性意见！");
            contributoryList.Add("tony2u", "第一个发现问题后，通过阅读农夫源码，并提出修改方案的朋友！");
            contributoryList.Add("农夫宝哥", "QQ：345543933。为农夫提供菜老伯测试数据！");
            contributoryList.Add("农夫测试人员", "监工老财、小马、越夜越寂寞、叶落随风、我爱我家、吴子享");
            contributoryList.Add("农夫交流群管理团队", "小冲、立飞、我爱我家");
            contributoryList.Add("  ", "");
            contributoryList.Add("   ", "");
            contributoryList.Add("    ", "");

            foreach (string key in contributoryList.Keys)
            {
                dgvContributory.Rows.Add(key, contributoryList[key]);
            }
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

        #region 花园农夫主逻辑

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

                txtWorkingBoard.Clear();

                inTimeTimer.Enabled = true;
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

                    ShowMsg(string.Format("\r\n[{0}] 操作被暂停！", DateTime.Now.ToString("HH:mm:ss")));
                    RecordFarmerWorkingLog(txtWorkingBoard.Text);

                    btnStop.Enabled = false;
                    btnStart.Enabled = true;
                    lblWorkingRemainingTime.Visible = false;
                    countDownTimer.Enabled = false;
                    lblNextWorkingTime.Visible = false;
                }
            }
            else
            {
                btnStop.Enabled = false;
                btnStart.Enabled = true;
                lblWorkingRemainingTime.Visible = false;
                countDownTimer.Enabled = false;
                lblNextWorkingTime.Visible = false;
            }
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

                Utility utility = new Utility();
                if (!utility.Login(workingAccountSetting.LoginEmail, workingAccountSetting.LoginPassword))
                {
                    ShowMsgWhileWorking(string.Format("登录失败！请检查帐号({0})配置的登录信息！", workingAccountSetting.LoginEmail));
                    continue;
                }

                ShowMsgWhileWorking("登录成功！正在进入我的花园...");
                GardenHelper helper = new GardenHelper(utility, gardenSetting.GlobalSetting.NetworkDelay);
                helper.GotoMyGarden();

                if (string.IsNullOrEmpty(workingAccountSetting.UID) || string.IsNullOrEmpty(workingAccountSetting.Name))
                {
                    workingAccountSetting.UID = utility.UID;
                    workingAccountSetting.Name = utility.Name;

                    GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);
                }

                #region 显示花园信息
                ShowMsgWhileWorking("正在读取花园信息...");
                GardenDetails gardenDetails = helper.GetGardenDetails(null);

                if (!string.IsNullOrEmpty(gardenDetails.ErrMsg))
                {
                    ShowMsgWhileWorking("读取花园信息失败：" + gardenDetails.ErrMsg);
                    ShowMsgWhileWorking("");

                    continue;
                }

                ShowMsgWhileWorking("农田信息：");
                DateTime minDT = DateTime.MaxValue;
                DateTime temp;
                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    ShowMsgWhileWorking(string.Format("{0}号农田：{1} {2}水{3} 害虫{4} {6}[{5}]",
                                                        gi.FarmNum,
                                                        string.IsNullOrEmpty(gi.Crops) ? "无农作物" : GetSeedName(gi.SeedId),
                                                        string.IsNullOrEmpty(gi.Crops) ? "" : gi.Crops.Replace("<br>", " ").Replace("<font size='12' color='#666666'>", "").Replace("</font>", "").Replace("<font color='#FF0000'>", "").Replace("<font>", ""),
                                                        gi.Water,
                                                        gi.Vermin,
                                                        GetCropStatusDesc(gi.CropsStatus),
                                                        gi.Grass == "1" ? "长草了" : ""));

                    if (gi.CropsStatus != "2" && workingAccountSetting.AutoHavestInTime)
                    {
                        if ((gi.Shared == "0" && workingAccountSetting.AutoHavest) || (gi.Shared == "1" && workingAccountSetting.AutoHavestHeartField))
                        {
                            temp = GetRipeTime(gi.Crops, gi.SeedId, false);
                            if (temp != DateTime.MaxValue)
                            {
                                if (temp < minDT)
                                {
                                    minDT = temp;
                                }
                            }
                        }
                    }
                }

                if (minDT <= DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval + 720) && workingAccountSetting.AutoHavestInTime)
                {
                    InTimeOperateItem o = new InTimeOperateItem();
                    o.UID = workingAccountSetting.UID;
                    o.Name = gardenDetails.Account.Name;
                    o.AccountSetting = workingAccountSetting;
                    o.IsSteal = false;
                    o.ActionTime = minDT;

                    AddInTimeOperateItem(o);
                    ShowInTimeNoticeInThread(string.Format("{0}: 预计自家花园[{1}]有果实成熟", gardenDetails.Account.Name, minDT.ToString("MM.dd HH:mm:ss")));
                }
                #endregion

                #region 显示用户情况
                ShowMsgWhileWorking("");
                ShowMsgWhileWorking(string.Format("{0}，{1}，魅力 {3}，{2}！", gardenDetails.Account.Name, gardenDetails.Account.RankTip, gardenDetails.Account.CashTip, gardenDetails.Account.TCharms));
                #endregion

                #region 显示仓库信息
                if (workingAccountSetting.AutoSell)
                {
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

                    if (myGranary.TotalPrice > 0)
                    {
                        SellResult sr = helper.SellFruit(true, null, null);

                        if (sr.Ret.ToLower() == "succ")
                        {
                            ShowMsgWhileWorking(string.Format("成功出售仓库里的所有农作物，收入 {0} 元！", sr.TotalPrice));
                        }
                        else
                        {
                            ShowMsgWhileWorking("出售农作物失败！！！");
                        }
                    }
                }

                #endregion

                #region 收获

                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    if (gi.CropsStatus == "2")
                    {
                        if ((gi.Shared == "0" && workingAccountSetting.AutoHavest) || (gi.Shared == "1" && workingAccountSetting.AutoHavestHeartField))
                        {
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
                        if (gi.Status == "1" && Convert.ToInt32(gi.Water) <= Convert.ToInt32(workingAccountSetting.WaterLowLimit) && gi.CropsId != "0")
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
                        //ShowMsgWhileWorking(string.Format("正在给{0}号农田犁地...", gi.FarmNum));
                        if (gi.CropsStatus == "3" || gi.CropsStatus == "-1")
                        {
                            if (gi.Shared == "0")
                            {
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

                List<MySeeds> mySeedsList = null;
                if (workingAccountSetting.AutoFarm)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsId == "0" && gi.Shared == "0" && gi.Status == "1")
                        {
                            string seedName = GetFarmingSeedName(workingAccountSetting, gardenDetails.Account.Rank, false);
                            SeedItem si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                if (!BuySeedForFarming(helper, ref mySeedsList, seedName, GetMaxNeededSeedNum(gardenDetails.GarderItems)))
                                {
                                    ShowMsgWhileWorking(string.Format("{0}号农田，种植失败！！！", gi.FarmNum));
                                }
                            }

                            si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                ShowMsgWhileWorking(string.Format("{0}号农田，种植失败！！！", gi.FarmNum));
                            }
                            else
                            {
                                FarmResult fr = helper.FarmSeed(gi.FarmNum, null, si.SeedID);
                                if (fr.Ret == "succ")
                                {
                                    ShowMsgWhileWorking(string.Format("{0}号农田，成功种植{1}！", gi.FarmNum, si.Name));
                                    gi.CropsId = "1";
                                    si.Num--;
                                }
                                else
                                {
                                    ShowMsgWhileWorking(string.Format("{0}号农田，种植{1}失败！！！{2}", gi.FarmNum, si.Name, fr.ErrMsg));
                                }
                            }
                        }
                    }
                }

                #endregion

                ShowMsgWhileWorking("");
                ShowMsgWhileWorking(string.Format("{0} 的花园工作完毕！", workingAccountSetting.LoginEmail));

                #region 去好友花园“做事”

                bool canFarmHeartFarm = true;

                Summary summary = GetSummary(workingAccountSetting.UID, workingAccountSetting.Name);
                UpdateSummaryInThread(summary);

                int minStealCropsPrice = GetCropsPrice(workingAccountSetting.StealCrops);

                for (int i = 0; i < workingAccountSetting.FriendSettings.Count; i++)
                {
                    FriendSetting friendSetting = workingAccountSetting.FriendSettings[i];

                    if (friendSetting.Steal || friendSetting.Plough || friendSetting.Water || friendSetting.Vermin || friendSetting.Farm || friendSetting.Grass)
                    {

                        ShowMsgWhileWorking("");

                        #region 读取好友花园信息

                        ShowMsgWhileWorking(string.Format("正在进入 {0} 的花园...", friendSetting.Name));
                        string html = helper.GotoFriendGarden(friendSetting.UID);

                        if (html == "1")
                        {
                            ShowMsgWhileWorking(string.Format("你和 {0} 已不再是好友，农夫已从配置中移除该好友！", friendSetting.Name));
                            workingAccountSetting.FriendSettings.Remove(friendSetting);
                            GardenSetting.SaveGardenSetting(Application.StartupPath, gardenSetting);
                            i--;

                            continue;
                        }

                        if (html.Contains("他还没有添加本组件"))
                        {
                            ShowMsgWhileWorking(string.Format("{0} 已经移除了买房子组件！", friendSetting.Name));
                            continue;
                        }

                        ShowMsgWhileWorking(string.Format("正在读取 {0} 花园信息...", friendSetting.Name));
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
                            // 若好友的花园里有菜老伯，且用户启用了“提防菜老伯”功能，则跳过偷取该好友的步骤
                            if (friendGardenDetails.Account.CareUrl != "" && workingAccountSetting.IsCare)
                            {
                                ShowMsgWhileWorking("");
                                ShowMsgWhileWorking(string.Format("{0} 的农田里有菜老伯，还是不偷了吧！", friendGardenDetails.Account.Name));

                                continue;
                            }

                            minDT = DateTime.MaxValue;
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.Crops.IndexOf("即将成熟") > 0)
                                {
                                    minDT = DateTime.Now.AddMinutes(1);
                                    continue;
                                }

                                // 若农田上的作物已成熟并且未偷过
                                if (gi.CropsStatus == "2" && !gi.Crops.Contains("已偷过"))
                                {
                                    // 若农田是自家田
                                    if (gi.Shared == "0")
                                    {
                                        if (gi.Crops.Contains("可偷"))
                                        {
                                            temp = GetReadyRipeTime(gi.Crops);
                                            if (temp != DateTime.MaxValue)
                                            {
                                                if (temp < minDT)
                                                {
                                                    minDT = temp;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // 若作物的售出价高于用户定义的价格，则偷
                                            if (GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice)
                                            {
                                                HavestResult hr = helper.Havest(gi.FarmNum, friendSetting.UID, null);

                                                if (hr.Ret == "succ")
                                                {
                                                    ShowMsgWhileWorking(string.Format("从好友的{0}号农田上偷取{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName));

                                                    summary.StealTimes++;
                                                    summary.StealedCropsNo += Convert.ToInt32(hr.Num);

                                                    UpdateSummaryInThread(summary);
                                                }
                                                else
                                                {
                                                    ShowMsgWhileWorking(string.Format("从好友的{0}号农田上偷取果实失败：{1}", gi.FarmNum, hr.Reason));
                                                }
                                            }
                                        }
                                    }
                                    else if (gi.Shared == "2")  // 若是爱心田
                                    {
                                        // 若该爱心田上的作物是用户种的，则执行收获操作
                                        if (gi.Farm.Contains(gardenDetails.Account.Name))
                                        {
                                            HavestResult hr = helper.Havest(gi.FarmNum, friendSetting.UID, null);

                                            if (hr.Ret == "succ")
                                            {
                                                ShowMsgWhileWorking(string.Format("从好友的{0}号爱心田上收获{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName));
                                                gi.CropsStatus = "3";
                                            }
                                            else
                                            {
                                                ShowMsgWhileWorking(hr.Reason);
                                            }
                                        }
                                        else  // 若该爱心田上的作物不是用户种的，则提示
                                        {
                                            ShowMsgWhileWorking(string.Format("{0}号农田，共种地不能偷！", gi.FarmNum));
                                        }
                                    }
                                }
                                else if (gi.CropsStatus == "1")  // 若农田上的作物未成熟
                                {
                                    // 若用户启动了“第一时间偷”功能
                                    if (workingAccountSetting.AutoStealInTime)
                                    {
                                        if ((gi.Shared == "0" && GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice) || (gi.Shared == "1" && gi.Farm.Contains(gardenDetails.Account.Name)))
                                        {
                                            temp = GetRipeTime(gi.Crops, gi.SeedId, true);
                                            if (temp != DateTime.MaxValue)
                                            {
                                                if (temp < minDT)
                                                {
                                                    minDT = temp;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // 若作物成熟的时间小于农夫的运行周期加两个小时，且用户启动了“第一时间偷取”功能
                            if (minDT <= DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval + 720) && workingAccountSetting.AutoStealInTime)
                            {
                                InTimeOperateItem o = new InTimeOperateItem();
                                o.IsSteal = true;
                                o.ActionTime = minDT;
                                o.AccountSetting = workingAccountSetting;
                                o.FUID = friendSetting.UID;
                                o.Name = gardenDetails.Account.Name;
                                o.UID = workingAccountSetting.UID;

                                AddInTimeOperateItem(o);
                                ShowInTimeNoticeInThread(string.Format("{0}: 预计{2}花园{1}有果实成熟", gardenDetails.Account.Name, minDT.ToString("MM-dd HH:mm:ss"), friendGardenDetails.Account.Name));
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
                                    if (helper.WaterFarm(gi.FarmNum, friendSetting.UID, null))
                                    {
                                        ShowMsgWhileWorking(string.Format("{0}号农田，浇水成功！", gi.FarmNum));

                                        summary.WaterTimes++;
                                        UpdateSummaryInThread(summary);
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

                                        summary.VerminTimes++;
                                        UpdateSummaryInThread(summary);
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

                        if (friendSetting.Plough)
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                //ShowMsgWhileWorking(string.Format("正在给{0}号农田犁地...", gi.FarmNum));
                                if (gi.CropsStatus == "3")
                                {
                                    if (gi.Shared == "1" || gi.Shared == "2")
                                    {
                                        if (helper.Plough(gi.FarmNum, friendSetting.UID, null))
                                        {
                                            ShowMsgWhileWorking(string.Format("{0}号爱心田，犁地成功！", gi.FarmNum));
                                            gi.CropsStatus = "";
                                            gi.CropsId = "0";
                                            gi.Shared = "1";
                                        }
                                        else
                                        {
                                            ShowMsgWhileWorking(string.Format("{0}号爱心田，犁地失败！！！", gi.FarmNum));
                                        }
                                    }
                                }
                            }

                        }

                        #endregion

                        #region 播种

                        if (friendSetting.Farm && gardenDetails.Account.Rank != "1")
                        {
                            foreach (GardenItem gi in friendGardenDetails.GarderItems)
                            {
                                if (gi.CropsId == "0" && gi.Shared == "1" && gi.Status == "1" && canFarmHeartFarm)
                                {
                                    string seedName = GetFarmingSeedName(workingAccountSetting, gardenDetails.Account.Rank, true);
                                    SeedItem si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                                    if (si == null)
                                    {
                                        if (!BuySeedForFarming(helper, ref mySeedsList, seedName, 1))
                                        {
                                            ShowMsgWhileWorking(string.Format("好友{0}号爱心农田，种植失败！！！", gi.FarmNum));
                                        }
                                    }

                                    si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                                    if (si == null)
                                    {
                                        ShowMsgWhileWorking(string.Format("好友{0}号爱心农田，种植失败！！！", gi.FarmNum));
                                    }
                                    else
                                    {
                                        FarmResult fr = helper.FarmSeed(gi.FarmNum, friendSetting.UID, si.SeedID);
                                        if (fr.Ret == "succ")
                                        {
                                            ShowMsgWhileWorking(string.Format("好友{0}号爱心农田，成功种植{1}！", gi.FarmNum, si.Name));
                                            si.Num--;
                                        }
                                        else
                                        {
                                            canFarmHeartFarm = false;
                                            ShowMsgWhileWorking(string.Format("好友{0}号爱心农田，种植{1}失败！！！{2}", gi.FarmNum, si.Name, fr.Reason));
                                        }
                                    }
                                }
                            }
                        }

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

                                        summary.GrassTimes++;
                                        UpdateSummaryInThread(summary);
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
                utility.Logout();
                ShowMsgWhileWorking("退出成功！");
                ShowMsgWhileWorking("");
            }

            RecordFarmerWorkingLog(txtWorkingBoard.Text);

            StartCountDownWhileWorking();
        }

        private int GetMaxNeededSeedNum(List<GardenItem> gardenItemList)
        {
            int count = 0;
            foreach (GardenItem gi in gardenItemList)
            {
                if (gi.CropsId == "0" && gi.Shared == "0" && gi.Status == "1")
                {
                    count++;
                }
            }
            return count;
        }

        private string GetFarmingSeedName(SNSHelper_Win_Garden.Entity.AccountSetting accountSetting, string rank, bool isHeart)
        {
            if (accountSetting.IsUsingPrivateSetting)
            {
                if (isHeart)
                {
                    return accountSetting.HeartCrops;
                }

                return accountSetting.Crops;
            }

            string seedName = SeedMVPHelper.GetSeedName(rank);

            if (string.IsNullOrEmpty(seedName))
            {
                return accountSetting.Crops;
            }

            return seedName;
        }

        string logFolderName = "Log";
        private void RecordFarmerWorkingLog(string log)
        {
            string logFileFolderPath = Path.Combine(Application.StartupPath, logFolderName);

            if (!Directory.Exists(logFileFolderPath))
            {
                Directory.CreateDirectory(logFileFolderPath);
            }

            StreamWriter sw = File.CreateText(Path.Combine(logFileFolderPath, string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss"))));

            sw.Write(log);
            sw.Flush();

            sw.Dispose();
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

        private bool BuySeedForFarming(GardenHelper helper, ref List<MySeeds> mySeedsList, string seedName, int count)
        {
            ShowMsgWhileWorking(string.Format("没{0}种子了，去商店购买...", seedName));

            string seedId = GetSeedID(seedName);
            if (helper.BuySeed(count, seedId))
            {
                ShowMsgWhileWorking(string.Format("成功购买{1}个{0}种子！", seedName, count));
                SeedItem si = new SeedItem();
                si.Name = seedName;
                si.SeedID = seedId;
                si.Num = count;

                AddSeedItemToMySeedsList(si, mySeedsList);
                return true;
            }
            else
            {
                ShowMsgWhileWorking(string.Format("购买{0}种子失败！", seedName));
                return false;
            }
        }

        private SeedItem GetSeedItemForFarming(GardenHelper helper, ref List<MySeeds> mySeedsList, string seedName)
        {
            if (mySeedsList == null)
            {
                mySeedsList = new List<MySeeds>();

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
            }

            return GetSeedItem(mySeedsList, seedName);
        }

        #region Show message

        private void ShowMsg(string msg)
        {
            if (txtWorkingBoard.Lines.Length > 1000)
            {
                RecordFarmerWorkingLog(txtWorkingBoard.Text);
                txtWorkingBoard.Clear();
            }
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
                msg = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), msg);
            }

            this.Invoke(showMsgWhileWorking, new object[] { msg });
        }

        #endregion

        #region Check new version & download new file


        private void biCheckUpdate_Click(object sender, EventArgs e)
        {
            isAutoUpdateFlag = false;
            BeginCheckUpdate();
        }

        private void BeginCheckUpdate()
        {
            if (checkUpdateThread == null)
            {
                checkUpdateThread = new Thread(CheckUpdate);
                checkUpdateThread.Start();
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

                if (!isAutoUpdateFlag)
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("检查更新失败，请稍候重试或到高兴网检查更新！", "提示");
                }

                ChangeFormTitleInThread(originalTitle + "   检查更新失败，请稍候重试！");

                checkUpdateThread = null;
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
                if (!isAutoUpdateFlag)
                {
                    DevComponents.DotNetBar.MessageBoxEx.Show("未发现新版本", "提示");
                }

                ChangeFormTitleInThread(originalTitle);
                checkUpdateThread = null;
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
                checkUpdateThread = null;
            }
        }

        string originalTitle;

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
                checkUpdateThread = null;
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

        private void biHelp_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "http://www.cnblogs.com/jailu/archive/2009/03/06/Farmer_Helper.html");
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
            if (farmerWorkingThread != null || downloadFileThread != null || checkUpdateThread != null || getNetFriendThread != null)
            {
                if (DevComponents.DotNetBar.MessageBoxEx.Show("农夫正在为你工作，你是否要退出本程序？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (farmerWorkingThread != null)
                    {
                        farmerWorkingThread.Abort();
                        farmerWorkingThread = null;
                    }

                    if (downloadFileThread != null)
                    {
                        downloadFileThread.Abort();
                        downloadFileThread = null;
                    }

                    if (checkUpdateThread != null)
                    {
                        checkUpdateThread.Abort();
                        checkUpdateThread = null;
                    }

                    if (getNetFriendThread != null)
                    {
                        getNetFriendThread.Abort();
                        getNetFriendThread = null;
                    }
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

            biFriendUp.Enabled = dgvFriendList.SelectedCells[0].RowIndex != 0;
            biFriendDown.Enabled = dgvFriendList.SelectedCells[0].RowIndex != dgvFriendList.Rows.Count - 1;
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

            InitSetting();
        }

        private void btnCropsCustomSetting_Click(object sender, EventArgs e)
        {
            frmCropsCustomSetting newfrm = new frmCropsCustomSetting();
            newfrm.ShowDialog();
        }

        private void buttonItem7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Application.StartupPath, "Log"));
        }

        private void biHeart_Click(object sender, EventArgs e)
        {
            //Process.Start("explorer.exe", "http://store.taobao.com/shop/view_shop-9e64c485791636107415f115111c5b9a.htm");
        }

        private void biHelp_Click_1(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "http://www.cnblogs.com/jailu/archive/2009/03/06/Farmer_Helper.html");
        }

        private void biUpdateSeedData_Click(object sender, EventArgs e)
        {
            if (gardenSetting.AccountSettings.Count == 0)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "您还没有添加任何帐号，无法启动种子库在线更新！", "提示");

                return;
            }

            if (farmerWorkingThread != null)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show(this, "农夫常规任务正在运行，为了避免不必要的错误，请先停止常规任务！", "提示");

                return;
            }

            tabControl1.SelectedTabIndex = 0;
            bar1.Enabled = false;
            btnStart.Enabled = false;

            txtWorkingBoard.Clear();
            stopUpdateSeedData = new MethodInvoker(StopUpdateSeedData);
            if (showMsgWhileWorking == null)
            {
                showMsgWhileWorking = new MethodWithParmString(ShowMsg);
            }

            updateSeedDataThread = new Thread(BeginUpdateSeedData);
            updateSeedDataThread.Start();
        }

        Thread updateSeedDataThread;
        private void BeginUpdateSeedData()
        {
            SNSHelper_Win_Garden.Entity.AccountSetting accountSetting = gardenSetting.AccountSettings[0];

            ShowMsgWhileWorking("农夫正在更新您的种子库信息！");
            ShowMsgWhileWorking("正在登录...");
            Utility _utility = new Utility();
            if (!_utility.Login(accountSetting.LoginEmail, accountSetting.LoginPassword))
            {
                ShowMsgWhileWorking("登录失败！");
                ShowMsgWhileWorking("更新种子库失败！");

                StopUpdateSeedDataInThread();
            }

            ShowMsgWhileWorking("正在进入花园...");
            GardenHelper helper = new GardenHelper(_utility, gardenSetting.GlobalSetting.NetworkDelay);
            helper.GotoMyGarden();

            ShowMsgWhileWorking("正在获取商店种子列表...");
            seedData = helper.GetSeedData();

            List<SeedItemInStore> seedItemInStoreList = new List<SeedItemInStore>();
            if (seedData != null)
            {
                foreach (SeedItem item in seedData.SeedItems)
                {
                    ShowMsgWhileWorking(string.Format("正在获取 {0} 的相关数据...", item.Name));
                    seedItemInStoreList.Add(helper.GetSeedItemInStore(item.SeedID));
                }
            }

            if (SaveSeedData(seedItemInStoreList))
            {
                ShowMsgWhileWorking("种子库更新操作成功！");
            }
            else
            {
                ShowMsgWhileWorking("种子库更新操作失败！");
            }
            StopUpdateSeedDataInThread();
        }

        private bool SaveSeedData(List<SeedItemInStore> seedItemInStoreList)
        {
            SeedData newSeedData = new SeedData();
            CropsIncomeHelper.CropsIncomeList = new List<CropsIncome>();

            foreach (SeedItemInStore item in seedItemInStoreList)
            {
                SeedItem si = new SeedItem();
                si.SeedID = GetSeedID(item.Name);
                si.Name = item.Name;
                si.FruitPic = item.FruitPic;
                si.Price = Convert.ToDouble(item.Price);

                newSeedData.SeedItems.Add(si);

                CropsIncome ci = new CropsIncome();
                ci.Name = item.Name;
                ci.GrowthCycle = Convert.ToInt32(item.MHours);
                ci.Theftproof = Convert.ToInt32(item.AntiStealDays);
                ci.UnitPrice = Convert.ToInt32(item.FruitPrice);

                CropsIncomeHelper.CropsIncomeList.Add(ci);
            }

            return SeedDataHelper.SaveSeedData(Application.StartupPath, seedData) && CropsIncomeHelper.SaveCropsIncome(Application.StartupPath, CropsIncomeHelper.CropsIncomeList);
        }

        MethodInvoker stopUpdateSeedData;
        private void StopUpdateSeedDataInThread()
        {
            this.Invoke(stopUpdateSeedData);
            updateSeedDataThread = null;
        }

        private void StopUpdateSeedData()
        {
            bar1.Enabled = true;
            btnStart.Enabled = true;

            stopUpdateSeedData = null;

            cbxCrops.Items.Clear();
            cbxStealCrops.Items.Clear();
            cbxHeartCrops.Items.Clear();
            foreach (SeedItem item in seedData.SeedItems)
            {
                cbxCrops.Items.Add(item.Name);
                cbxStealCrops.Items.Add(item.Name);
                cbxHeartCrops.Items.Add(item.Name);
            }
        }

        private void 官方交流论坛会员火热抢注中_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "http://bbs.jailu.cn");
        }
    }
}
