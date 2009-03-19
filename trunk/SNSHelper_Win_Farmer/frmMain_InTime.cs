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
        List<InTimeOperateItem> inTimeOperateItemList = new List<InTimeOperateItem>();

        private void inTimeTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < inTimeOperateItemList.Count; i++)
            {
                if (inTimeOperateItemList[i].ActionTime <= DateTime.Now)
                {
                    if (!inTimeOperateItemList[i].IsSteal)
                    {
                        if (!inTimeOperateItemList[i].IsRunning)
                        {
                            inTimeOperateItemList[i].IsRunning = true;
                            // 及时收获
                            ThreadPool.QueueUserWorkItem(HavestInTime, inTimeOperateItemList[i]);
                        }
                    }
                    else
                    {
                        if (!inTimeOperateItemList[i].IsRunning)
                        {
                            inTimeOperateItemList[i].IsRunning = true;
                            // 及时偷窃
                            ThreadPool.QueueUserWorkItem(StealInTime, inTimeOperateItemList[i]);
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
            InTimeOperateItem inTimeOperateItem = o as InTimeOperateItem;

            Utility _utility = new Utility();

            if (_utility.Login(inTimeOperateItem.LoginEmail, inTimeOperateItem.LoginPsw))
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
                        if ((gi.Shared == "0" && inTimeOperateItem.AccountSetting.AutoHavest) || (gi.Shared == "1" && inTimeOperateItem.AccountSetting.AutoHavestHeartField))
                        {
                            HavestResult hr = helper.Havest(gi.FarmNum, null, null);

                            if (hr.Ret == "succ")
                            {
                                ShowInTimeMsgInThread(string.Format("{3}：从{0}号农田上收获{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName, inTimeOperateItem.LoginEmail));
                                gi.CropsStatus = "3";
                            }
                            else
                            {
                                ShowInTimeMsgInThread(string.Format("{2}：从{0}号农田上收获{1}失败！{3}", gi.FarmNum, hr.SeedName, inTimeOperateItem.LoginEmail, hr.Reason));
                            }
                        }
                    }

                    if (gi.CropsStatus != "2")
                    {
                        if ((gi.Shared == "0" && inTimeOperateItem.AccountSetting.AutoHavest) || (gi.Shared == "1" && inTimeOperateItem.AccountSetting.AutoHavestHeartField))
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

                    if (gi.Crops.IndexOf("即将成熟") > 0)
                    {
                        isReadyRipe = true;
                    }
                }
                #endregion

                #region 犁地

                if (inTimeOperateItem.AccountSetting.AutoPlough)
                {
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsStatus == "3" || gi.CropsStatus == "-1")
                        {
                            if (gi.Shared == "0")
                            {
                                if (helper.Plough(gi.FarmNum, null, null))
                                {
                                    ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，犁地成功！", gi.FarmNum, inTimeOperateItem.LoginEmail));
                                    gi.CropsStatus = "";
                                    gi.CropsId = "0";
                                }
                                else
                                {
                                    ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，犁地失败！！！", gi.FarmNum, inTimeOperateItem.LoginEmail));
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 播种

                if (inTimeOperateItem.AccountSetting.AutoFarm)
                {
                    List<MySeeds> mySeedsList = null;
                    foreach (GardenItem gi in gardenDetails.GarderItems)
                    {
                        if (gi.CropsId == "0" && gi.Shared == "0" && gi.Status == "1")
                        {
                            string seedName = GetFarmingSeedName(inTimeOperateItem.AccountSetting, gardenDetails.Account.Rank, false);
                            SeedItem si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                if (!BuySeedForFarming(helper, ref mySeedsList, seedName, GetMaxNeededSeedNum(gardenDetails.GarderItems)))
                                {
                                    ShowMsgWhileWorking(string.Format("{1}：{0}号农田，种植失败！！！", gi.FarmNum, inTimeOperateItem.LoginEmail));
                                }
                            }

                            si = GetSeedItemForFarming(helper, ref mySeedsList, seedName);

                            if (si == null)
                            {
                                ShowInTimeMsgInThread(string.Format("{1}：{0}号农田，种植失败！！！", gi.FarmNum, inTimeOperateItem.LoginEmail));
                            }
                            else
                            {
                                FarmResult fr = helper.FarmSeed(gi.FarmNum, null, si.SeedID);
                                if (fr.Ret == "succ")
                                {
                                    ShowInTimeMsgInThread(string.Format("{2}：{0}号农田，成功种植{1}！", gi.FarmNum, si.Name, inTimeOperateItem.LoginEmail));
                                    si.Num--;
                                }
                                else
                                {
                                    ShowInTimeMsgInThread(string.Format("{3}：{0}号农田，种植{1}失败！！！{2}", gi.FarmNum, si.Name, fr.ErrMsg, inTimeOperateItem.LoginEmail));
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
                    if (minDT <= DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval + 720))
                    {
                        inTimeOperateItem.ActionTime = minDT;
                        inTimeOperateItem.IsRunning = false;

                        AddInTimeOperateItem(inTimeOperateItem);
                        ShowInTimeNoticeInThread(string.Format("{0}: 预计自家花园[{1}]有果实成熟", gardenDetails.Account.Name, minDT.ToString("MM.dd HH:mm:ss")));
                    }
                    else
                    {
                        DeleteInTimeObject(inTimeOperateItem);
                    }
                }
            }
        }

        private void StealInTime(object o)
        {
            InTimeOperateItem inTimeOperateItem = o as InTimeOperateItem;

            Utility _utility = new Utility();

            if (_utility.Login(inTimeOperateItem.LoginEmail, inTimeOperateItem.LoginPsw))
            {
                GardenHelper helper = new GardenHelper(_utility, gardenSetting.GlobalSetting.NetworkDelay);
                helper.GotoFriendGarden(inTimeOperateItem.FUID);

                DateTime minDT = DateTime.MaxValue;
                DateTime temp;

            GetFriendGardenDetails:
                #region
                bool isReadyRipe = false;
                GardenDetails gardenDetails = helper.GetGardenDetails(inTimeOperateItem.FUID);

                ShowInTimeMsgInThread(string.Format("已获取 {0} 的花园数据！", gardenDetails.Account.Name));
                Summary summary = GetSummary(inTimeOperateItem.Name);

                if (gardenDetails.ErrMsg == "1")
                {
                    ShowInTimeMsgInThread(string.Format("你和 {0} 已不再是好友，农夫已从配置中移除该好友！", gardenDetails.Account.Name));
                    DeleteInTimeObject(inTimeOperateItem);

                    return;
                }

                int minStealCropsPrice = GetCropsPrice(inTimeOperateItem.AccountSetting.StealCrops);

                // 若好友的花园里有菜老伯，且用户启用了“提防菜老伯”功能，则跳过偷取该好友的步骤
                if (gardenDetails.Account.CareUrl != "" && inTimeOperateItem.AccountSetting.IsCare)
                {
                    ShowInTimeMsgInThread("");
                    ShowInTimeMsgInThread(string.Format("{0} 的农田里有菜老伯，还是不偷了吧！", gardenDetails.Account.Name));
                }

                minDT = DateTime.MaxValue;
                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    if (gi.Crops.IndexOf("即将成熟") > 0)
                    {
                        isReadyRipe = true;
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
                                ShowInTimeMsgInThread(string.Format("{0} 号农田的果实有防偷期，预计在{1}可偷！", gi.FarmNum, temp.ToString("MM-dd HH:mm:ss")));
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
                                    HavestResult hr = helper.Havest(gi.FarmNum, inTimeOperateItem.FUID, null);

                                    if (hr.Ret == "succ")
                                    {
                                        ShowInTimeMsgInThread(string.Format("{3}：从{4}的{0}号农田上偷取{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName, inTimeOperateItem.Name, gardenDetails.Account.Name));

                                        summary.StealTimes++;
                                        summary.StealedCropsNo += Convert.ToInt32(hr.Num);
                                    }
                                    else
                                    {
                                        ShowInTimeMsgInThread(string.Format("{0}：从{1}的{2}号农田上偷取失败。原因：{3}", inTimeOperateItem.Name, gardenDetails.Account.Name, gi.FarmNum, hr.Reason));
                                    }
                                }
                            }
                        }
                        else if (gi.Shared == "2")  // 若是爱心田
                        {
                            // 若该爱心田上的作物是用户种的，则执行收获操作
                            if (gi.Farm.Contains(gardenDetails.Account.Name))
                            {
                                HavestResult hr = helper.Havest(gi.FarmNum, inTimeOperateItem.FUID, null);

                                if (hr.Ret == "succ")
                                {
                                    ShowInTimeMsgInThread(string.Format("{0}：从{1}的{2}号爱心田上收获{3}个{4}！", inTimeOperateItem.Name, gardenDetails.Account.Name, gi.FarmNum, hr.Num, hr.SeedName));
                                    gi.CropsStatus = "3";
                                }
                                else
                                {
                                    ShowInTimeMsgInThread(string.Format("{0}：从{1}的{2}号爱心田上收获{3}失败：{4}", inTimeOperateItem.Name, gardenDetails.Account.Name, gi.FarmNum, hr.SeedName, hr.Reason));
                                }
                            }
                            else  // 若该爱心田上的作物不是用户种的，则提示
                            {
                                ShowInTimeMsgInThread(string.Format("{0}：{1}的{2}号农田，共种地不能偷！", inTimeOperateItem.Name, gardenDetails.Account.Name, gi.FarmNum));
                            }
                        }
                    }
                    else if (gi.CropsStatus == "1")  // 若农田上的作物未成熟
                    {
                        // 若用户启动了“第一时间偷”功能
                        if (inTimeOperateItem.AccountSetting.AutoStealInTime)
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

                if (isReadyRipe)
                {
                    ShowInTimeMsgInThread(string.Format("{0} 花园里的果实就要成熟了，5秒钟后再次尝试偷取！", gardenDetails.Account.Name));
                    Thread.Sleep(5000);
                    goto GetFriendGardenDetails;
                }

                // 若作物成熟的时间小于农夫的运行周期加两个小时，且用户启动了“第一时间偷取”功能
                if (minDT <= DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval + 720) && inTimeOperateItem.AccountSetting.AutoStealInTime)
                {
                    inTimeOperateItem.ActionTime = minDT;
                    inTimeOperateItem.IsRunning = false;

                    AddInTimeOperateItem(inTimeOperateItem);
                    ShowInTimeNoticeInThread(string.Format("{0}: 预计{2}花园{1}有果实成熟", gardenDetails.Account.Name, minDT.ToString("MM-dd HH:mm:ss"), gardenDetails.Account.Name));
                }

                #region
                //if (!string.IsNullOrEmpty(gardenDetails.Account.CareUrl) && inTimeOperateItem.AccountSetting.IsCare)
                //{
                //    ShowInTimeMsgInThread(string.Format("{0} 的农田里有菜老伯，还是不偷了吧！", gardenDetails.Account.Name));
                //    DeleteInTimeObject(inTimeOperateItem);

                //    return;
                //}

                //foreach (GardenItem gi in gardenDetails.GarderItems)
                //{
                //    if (gi.Crops.IndexOf("即将成熟") > 0)
                //    {
                //        isReadyRipe = true;

                //        continue;
                //    }

                //    if (gi.CropsStatus == "2" && !gi.Crops.Contains("已偷过") && gi.Shared == "0")
                //    {
                //        if (gi.Crops.Contains("可偷"))
                //        {
                //            temp = GetReadyRipeTime(gi.Crops);
                //            if (temp != DateTime.MaxValue)
                //            {
                //                if (temp < minDT)
                //                {
                //                    minDT = temp;
                //                }
                //            }

                //            continue;
                //        }

                //        if (GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice)
                //        {
                //            HavestResult hr = helper.Havest(gi.FarmNum, inTimeOperateItem.FUID, null);

                //            if (hr.Ret == "succ")
                //            {
                //                ShowInTimeMsgInThread(string.Format("{3}：从{4}的{0}号农田上偷取{1}个{2}！", gi.FarmNum, hr.Num, hr.SeedName, inTimeOperateItem.LoginEmail, gardenDetails.Account.Name));

                //                summary.StealTimes++;
                //                summary.StealedCropsNo += Convert.ToInt32(hr.Num);
                //            }
                //            else
                //            {
                //                ShowInTimeMsgInThread(string.Format("{0}：从{1}的{2}号农田上偷取失败。原因：{3}", inTimeOperateItem.LoginEmail, gardenDetails.Account.Name, gi.FarmNum, hr.Reason));
                //            }

                //            continue;
                //        }

                //        continue;
                //    }

                //    if (gi.CropsStatus != "2" && ((gi.Shared == "0" && GetCropsPrice(GetSeedName(gi.SeedId)) >= minStealCropsPrice) || (gi.Shared == "1" && gi.Farm.Contains(gardenDetails.Account.Name))))
                //    {
                //        temp = GetRipeTime(gi.Crops, gi.SeedId, true);
                //        if (temp != DateTime.MaxValue)
                //        {
                //            if (temp < minDT)
                //            {
                //                minDT = temp;
                //            }
                //        }
                //    }
                //}

                //if (isReadyRipe)
                //{
                //    Thread.Sleep(10000);
                //    goto GetFriendGardenDetails;
                //}
                //else
                //{
                //    if (minDT <= DateTime.Now.AddMinutes(gardenSetting.GlobalSetting.WorkingInterval + 720))
                //    {
                //        inTimeOperateItem.ActionTime = minDT;
                //        inTimeOperateItem.IsRunning = false;

                //        AddInTimeOperateItem(inTimeOperateItem);
                //        ShowInTimeNoticeInThread(string.Format("{0}: 预计{2}花园{1}有果实成熟", inTimeOperateItem.Name, minDT.ToString("MM-dd HH:mm:ss"), gardenDetails.Account.Name));
                //    }
                //    else
                //    {
                //        DeleteInTimeObject(inTimeOperateItem);
                //    }
                //}
                #endregion

                UpdateSummaryInThread(summary);

                #endregion
            }
        }

        private DateTime GetRipeTime(string crops, string seedId, bool isSteal)
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

                    if (isSteal)
                    {
                        return dt.AddSeconds(10).AddHours(CropsIncomeHelper.GetCropsIncome(GetSeedName(seedId)).Theftproof);
                    }
                    else
                    {
                        return dt.AddSeconds(10);
                    }
                }
            }

            return DateTime.MaxValue;
        }

        private DateTime GetReadyRipeTime(string crops)
        {
            string pattern = @"再过(?:(?<day>\d+)天)?(?:(?<hour>\d+)小时)?(?:(?<minute>\d+)分?)?(?:(?<second>\d+)秒?)?可偷";
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

                    return dt.AddSeconds(30);
                }
            }

            return DateTime.MaxValue;
        }

        private void AddInTimeOperateItem(InTimeOperateItem inTimeOperateItem)
        {
            lock (inTimeOperateItemList)
            {
                for (int i = 0; i < inTimeOperateItemList.Count; i++)
                {
                    // 若第一时间操作不是“偷”
                    if (!inTimeOperateItem.IsSteal)
                    {
                        // 若操作所属帐号相同
                        if (inTimeOperateItemList[i].LoginEmail == inTimeOperateItem.LoginEmail)
                        {
                            inTimeOperateItemList[i].ActionTime = inTimeOperateItem.ActionTime;

                            return;
                        }
                    }
                    else
                    {
                        // 若操作所属帐号相同；好友ID相同
                        if (inTimeOperateItemList[i].LoginEmail == inTimeOperateItem.LoginEmail && inTimeOperateItemList[i].FUID == inTimeOperateItem.FUID)
                        {
                            inTimeOperateItemList[i].ActionTime = inTimeOperateItem.ActionTime;

                            return;
                        }
                    }
                }

                inTimeOperateItemList.Add(inTimeOperateItem);
            }
        }

        private void DeleteInTimeObject(InTimeOperateItem inTimeObject)
        {
            lock (inTimeOperateItemList)
            {
                for (int i = 0; i < inTimeOperateItemList.Count; i++)
                {
                    if (inTimeOperateItemList[i].LoginEmail == inTimeObject.LoginEmail)
                    {
                        inTimeOperateItemList.RemoveAt(i);

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
                //RecordFarmerWorkingLog(txtInTimeBoard.Text);
                txtInTimeBoard.Clear();
            }

            txtInTimeBoard.AppendText(msg);
        }

        MethodWithParmString showInTimeNoticeInThread;
        private void ShowInTimeNoticeInThread(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = "\r\n";
            }
            else
            {
                msg = string.Format("{0}\r\n", msg);
            }

            this.Invoke(showInTimeNoticeInThread, new object[] { msg });
        }

        private void ShowInTimeNotice(string msg)
        {
            if (txtInTimeNotice.Lines.Length > 1000)
            {
                //RecordFarmerWorkingLog(txtInTimeBoard.Text);
                txtInTimeNotice.Clear();
            }

            txtInTimeNotice.AppendText(msg);
        }

        class InTimeOperateItem
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

            private string name = string.Empty;
            public string Name
            {
                get
                {
                    return name;
                }
                set
                {
                    name = value;
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

            private DateTime actionTime = DateTime.Now;
            public DateTime ActionTime
            {
                get
                {
                    return actionTime;
                }
                set
                {
                    actionTime = value;
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
