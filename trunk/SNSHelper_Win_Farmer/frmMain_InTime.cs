using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Garden;
using System.Text.RegularExpressions;
using SNSHelper_Win_Garden.Entity;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        List<InTimeItem> inTimeItemList = new List<InTimeItem>();

        private void inTimeTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < inTimeItemList.Count; i++)
            {
                if (inTimeItemList[i].ActiveTime <= DateTime.Now)
                {
                    if (!inTimeItemList[i].IsSteal)
                    {
                        if (!inTimeItemList[i].IsRunning)
                        {
                            inTimeItemList[i].IsRunning = true;
                            // 及时收获
                            ThreadPool.QueueUserWorkItem(HavestInTime, inTimeItemList[i]);
                        }
                    }
                    else
                    {
                        if (!inTimeItemList[i].IsRunning)
                        {
                            inTimeItemList[i].IsRunning = true;
                            // 及时偷窃
                            ThreadPool.QueueUserWorkItem(StealInTime, inTimeItemList[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 第一时间收获
        /// </summary>
        /// <param name="o">InTimeItem对象</param>
        private void HavestInTime(object o)
        {
            InTimeItem inTimeObject = o as InTimeItem;

            Utility _utility = new Utility();

            if (_utility.Login(inTimeObject.LoginEmail, inTimeObject.LoginPsw))
            {
                GardenHelper helper = new GardenHelper(_utility, gardenSetting.GlobalSetting.NetworkDelay);
                helper.GotoMyGarden();

            GetGardenDetails:
                bool isReadyRipe = false;
                GardenDetails gardenDetails = helper.GetGardenDetails(null);

                DateTime minDT = DateTime.MaxValue;
                DateTime temp;

                #region 收获
                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    if (gi.CropsStatus == "2")
                    {
                        if ((gi.Shared == "0" && inTimeObject.AccountSetting.AutoHavest) || (gi.Shared == "1" && inTimeObject.AccountSetting.AutoHavestHeartField))
                        {
                            HavestResult hr = helper.Havest(gi.FarmNum, null, null);

                            if (hr.Ret == "succ")
                            {
                                ShowInTimeMsgInThread(string.Format("{3}：从{0}号农田上收获{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName, inTimeObject.LoginEmail));
                                gi.CropsStatus = "3";
                            }
                            else
                            {
                                ShowInTimeMsgInThread(hr.Reason);
                            }
                        }
                    }


                    if (gi.CropsStatus != "2")
                    {
                        if ((gi.Shared == "0" && inTimeObject.AccountSetting.AutoHavest) || (gi.Shared == "1" && inTimeObject.AccountSetting.AutoHavestHeartField))
                        {
                            temp = GetRipeTime(gi.Crops, gi.SeedId);
                            if (temp != DateTime.MaxValue)
                            {
                                if (temp < minDT)
                                {
                                    minDT = temp;
                                }
                            }
                        }
                    }

                    if (gi.Crops.IndexOf("即将成熟") > 0)
                    {
                        isReadyRipe = true;
                    }
                }
                #endregion

                #region 犁地

                if (inTimeObject.AccountSetting.AutoPlough)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsStatus == "3" || gi.CropsStatus == "-1")
                        {
                            if (gi.Shared == "0")
                            {
                                if (helper.Plough(gi.FarmNum, null, null))
                                {
                                    ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，犁地成功！", gi.FarmNum, inTimeObject.LoginEmail));
                                    gi.CropsStatus = "";
                                    gi.CropsId = "0";
                                }
                                else
                                {
                                    ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，犁地失败！！！", gi.FarmNum, inTimeObject.LoginEmail));
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 播种

                if (inTimeObject.AccountSetting.AutoFarm)
                {
                    List<MySeeds> mySeedsList = null;
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsId == "0" && gi.Shared == "0" && gi.Status == "1")
                        {
                            string seedName = GetFarmingSeedName(inTimeObject.AccountSetting, gardenDetails.Account.Rank, false);
                            SeedItem si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                if (!BuySeedForFarming(helper, ref mySeedsList, seedName, GetMaxNeededSeedNum(gardenDetails.GarderItems)))
                                {
                                    ShowMsgWhileWorking(string.Format("{1}：{0}号农田，种植失败！！！", gi.FarmNum, inTimeObject.LoginEmail));
                                }
                            }

                            si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，种植失败！！！", gi.FarmNum, inTimeObject.LoginEmail));
                            }
                            else
                            {
                                FarmResult fr = helper.FarmSeed(gi.FarmNum, null, si.SeedID);
                                if (fr.Ret == "succ")
                                {
                                    ShowInTimeMsgInThread(string.Format("{2}：{0}号农田，成功种植{1}！", gi.FarmNum, si.Name, inTimeObject.LoginEmail));
                                    si.Num--;
                                }
                                else
                                {
                                    ShowInTimeMsgInThread(string.Format("{3}：{0}号农田，种植{1}失败！！！{2}", gi.FarmNum, si.Name, fr.ErrMsg, inTimeObject.LoginEmail));
                                }
                            }
                        }
                    }
                }

                #endregion

                if (isReadyRipe)
                {
                    Thread.Sleep(10000);
                    goto GetGardenDetails;
                }
                else
                {
                    if (minDT != DateTime.MaxValue)
                    {
                        inTimeObject.ActiveTime = minDT;
                        inTimeObject.IsRunning = false;

                        AddInTimeObject(inTimeObject);
                    }
                    else
                    {
                        DeleteInTimeObject(inTimeObject);
                    }
                }
            }
        }

        private void StealInTime(object o)
        {
            InTimeItem inTimeObject = o as InTimeItem;

            Utility _utility = new Utility();

            if (_utility.Login(inTimeObject.LoginEmail, inTimeObject.LoginPsw))
            {
                GardenHelper helper = new GardenHelper(_utility, gardenSetting.GlobalSetting.NetworkDelay);
                helper.GotoMyGarden();

                DateTime minDT = DateTime.MaxValue;
                DateTime temp;

            GetFriendGardenDetails:
                #region
                bool isReadyRipe = false;
                GardenDetails gardenDetails = helper.GetGardenDetails(inTimeObject.FUID);

                Summary summary = GetSummary(gardenDetails.Account.Name);

                if (gardenDetails.ErrMsg == "1")
                {
                    ShowInTimeMsgInThread(string.Format("你和 {0} 已不再是好友，农夫已从配置中移除该好友！", gardenDetails.Account.Name));
                    DeleteInTimeObject(inTimeObject);

                    return;
                }

                int minStealCropsPrice = GetCropsPrice(inTimeObject.AccountSetting.StealCrops);

                if (!string.IsNullOrEmpty(gardenDetails.Account.CareUrl) && inTimeObject.AccountSetting.IsCare)
                {
                    ShowInTimeMsgInThread(string.Format("{0} 的农田里有菜老伯，还是不偷了吧！", gardenDetails.Account.Name));
                    DeleteInTimeObject(inTimeObject);

                    return;
                }

                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    if (gi.Crops.IndexOf("即将成熟") > 0)
                    {
                        isReadyRipe = true;
                    }

                    if (gi.CropsStatus == "2" && !gi.Crops.Contains("已偷过") && gi.Shared == "0")
                    {
                        if (gi.Shared != "0")
                        {
                            //ShowMsgWhileWorking(string.Format("{0}号农田，共种地不能偷！", gi.FarmNum));

                            continue;
                        }

                        if (GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice)
                        {
                            HavestResult hr = helper.Havest(gi.FarmNum, inTimeObject.FUID, null);

                            if (hr.Ret == "succ")
                            {
                                ShowInTimeMsgInThread(string.Format("{3}：从{4}的{0}号农田上偷取{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName, inTimeObject.LoginEmail, gardenDetails.Account.Name));

                                summary.StealTimes++;
                                summary.StealedCropsNo += Convert.ToInt32(hr.Num);
                            }
                            else
                            {
                                ShowInTimeMsgInThread(string.Format("{0}：从{1}的{2}号农田上偷取失败。原因：{3}", inTimeObject.LoginEmail, gardenDetails.Account.Name, gi.FarmNum, hr.Reason));
                            }
                        }
                    }

                    if (gi.CropsStatus != "2" && GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice)
                    {
                        temp = GetRipeTime(gi.Crops, gi.SeedId);
                        if (temp != DateTime.MaxValue)
                        {
                            if (temp < minDT)
                            {
                                minDT = temp;
                            }
                        }
                    }
                }

                if (isReadyRipe)
                {
                    Thread.Sleep(10000);
                    goto GetFriendGardenDetails;
                }
                else
                {
                    if (minDT != DateTime.MaxValue)
                    {
                        inTimeObject.ActiveTime = minDT;
                        inTimeObject.IsRunning = false;

                        AddInTimeObject(inTimeObject);
                    }
                    else
                    {
                        DeleteInTimeObject(inTimeObject);
                    }
                }

                UpdateSummaryInThread(summary);

                #endregion
            }
        }

        private DateTime GetRipeTime(string crops, string seedId)
        {
            string pattern = @"距离收获：(?:(?<day>\d+)天)?(?:(?<hour>\d+)小时)?(?:(?<minute>\d+)分?)?(?:(?<second>\d+)秒?)?";
            Regex reg = new Regex(pattern);
            MatchCollection mc = reg.Matches(crops);
            foreach (Match match in mc)
            {
                if (match.Success)
                {
                    DateTime dt = DateTime.Now;
                    if (!string.IsNullOrEmpty(match.Groups["day"].Value))
                    {
                        dt = dt.AddDays(Convert.ToDouble(match.Groups["day"].Value));
                    }
                    if (!string.IsNullOrEmpty(match.Groups["hour"].Value))
                    {
                        dt = dt.AddHours(Convert.ToDouble(match.Groups["hour"].Value));
                    }
                    if (!string.IsNullOrEmpty(match.Groups["minute"].Value))
                    {
                        dt = dt.AddMinutes(Convert.ToDouble(match.Groups["minute"].Value));
                    }
                    if (!string.IsNullOrEmpty(match.Groups["second"].Value))
                    {
                        dt = dt.AddSeconds(Convert.ToDouble(match.Groups["second"].Value));
                    }

                    return dt.AddSeconds(30).AddHours(CropsIncomeHelper.GetCropsIncome(GetSeedName(seedId)).UnitPrice);
                }
            }

            return DateTime.MaxValue;
        }

        private void AddInTimeObject(InTimeItem inTimeObject)
        {
            lock (inTimeItemList)
            {
                for (int i = 0; i < inTimeItemList.Count; i++)
                {
                    if (string.IsNullOrEmpty(inTimeObject.FUID))
                    {
                        if (inTimeItemList[i].LoginEmail == inTimeObject.LoginEmail)
                        {
                            inTimeItemList[i].ActiveTime = inTimeObject.ActiveTime;

                            return;
                        }
                    }
                    else
                    {
                        if (inTimeItemList[i].FUID == inTimeObject.FUID && inTimeItemList[i].LoginEmail == inTimeObject.LoginEmail)
                        {
                            inTimeItemList[i].ActiveTime = inTimeObject.ActiveTime;

                            return;
                        }
                    }
                }

                inTimeItemList.Add(inTimeObject);
            }
        }

        private void DeleteInTimeObject(InTimeItem inTimeObject)
        {
            lock (inTimeItemList)
            {
                for (int i = 0; i < inTimeItemList.Count; i++)
                {
                    if (inTimeItemList[i].LoginEmail == inTimeObject.LoginEmail)
                    {
                        inTimeItemList.RemoveAt(i);

                        return;
                    }
                }
            }
        }

        MethodWithParmString showInTimeMsgInThread;
        private void ShowInTimeMsgInThread(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "\r\n";
            }
            else
            {
                msg = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), msg);
            }

            this.Invoke(showInTimeMsgInThread, new object[] { msg });
        }

        private void ShowInTimeMsg(string msg)
        {
            if (txtInTimeBoard.Lines.Length > 1000)
            {
                RecordFarmerWorkingLog(txtInTimeBoard.Text);
                txtInTimeBoard.Clear();
            }

            txtInTimeBoard.AppendText(msg);
        }

        class InTimeItem
        {
            private SNSHelper_Win_Garden.Entity.AccountSetting accountSetting = new SNSHelper_Win_Garden.Entity.AccountSetting();
            public SNSHelper_Win_Garden.Entity.AccountSetting AccountSetting
            {
                get
                {
                    return accountSetting;
                }
                set
                {
                    accountSetting.AutoBuySeed = value.AutoBuySeed;
                    accountSetting.AutoFarm = value.AutoFarm;
                    accountSetting.AutoGrass = value.AutoGrass;
                    accountSetting.AutoHavest = value.AutoHavest;
                    accountSetting.AutoHavestInTime = value.AutoHavestInTime;
                    accountSetting.AutoPlough = value.AutoPlough;
                    accountSetting.AutoSell = value.AutoSell;
                    accountSetting.AutoVermin = value.AutoVermin;
                    accountSetting.AutoWater = value.AutoWater;
                    accountSetting.Crops = value.Crops;
                    accountSetting.IsOperate = value.IsOperate;
                    accountSetting.IsUsingPrivateSetting = value.IsUsingPrivateSetting;
                    accountSetting.LoginEmail = value.LoginEmail;
                    accountSetting.LoginPassword = value.LoginPassword;
                    accountSetting.Seed = value.Seed;
                    accountSetting.StealCrops = value.StealCrops;
                    accountSetting.WaterLowLimit = value.WaterLowLimit;
                    accountSetting.IsCare = value.IsCare;
                }
            }

            public string LoginEmail
            {
                get
                {
                    return accountSetting.LoginEmail;
                }
            }

            public string LoginPsw
            {
                get
                {
                    return accountSetting.LoginPassword;
                }
            }

            private string fuid = string.Empty;
            public string FUID
            {
                get
                {
                    return fuid;
                }
                set
                {
                    fuid = value;
                }
            }

            private bool isSteal = false;
            public bool IsSteal
            {
                get
                {
                    return isSteal;
                }
                set
                {
                    isSteal = value;
                }
            }

            private DateTime activeTime = DateTime.Now;
            public DateTime ActiveTime
            {
                get
                {
                    return activeTime;
                }
                set
                {
                    activeTime = value;
                }
            }

            private bool isRunning = false;
            public bool IsRunning
            {
                get
                {
                    return isRunning;
                }
                set
                {
                    isRunning = value;
                }
            }
        }
    }
}
