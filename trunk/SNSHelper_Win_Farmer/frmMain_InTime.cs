using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SNSHelper.Kaixin001;
using SNSHelper.Kaixin001.Entity.Garden;
using System.Text.RegularExpressions;

namespace SNSHelper_Win_Garden
{
    public partial class frmMain : DevComponents.DotNetBar.Office2007Form
    {
        List<InTimeObject> inTimeObjectList = new List<InTimeObject>();

        private void inTimeTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < inTimeObjectList.Count; i++)
            {
                if (inTimeObjectList[i].ActiveTime <= DateTime.Now)
                {
                    if (!inTimeObjectList[i].IsSteal)
                    {
                        if (!inTimeObjectList[i].IsRunning)
                        {
                            inTimeObjectList[i].IsRunning = true;
                            // 及时收获
                            ThreadPool.QueueUserWorkItem(HavestInTime, inTimeObjectList[i]);
                        }
                    }
                    else
                    {
                        // 及时偷窃
                        ThreadPool.QueueUserWorkItem(StealInTime, inTimeObjectList[i]);
                    }
                }
            }
        }

        private void HavestInTime(object o)
        {
            InTimeObject inTimeObject = o as InTimeObject;

            Utility _utility = new Utility();

            if (_utility.Login(inTimeObject.LoginEmail, inTimeObject.LoginPsw))
            {
                GardenHelper helper = new GardenHelper(_utility);
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


                    if (gi.CropsStatus != "2")
                    {
                        temp = GetRipeTime(gi.Crops);
                        if (temp != DateTime.MaxValue)
                        {
                            if (temp < minDT)
                            {
                                minDT = temp;
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

                #endregion

                #region 播种

                List<MySeeds> mySeedsList = null;
                foreach (GardenItem gi in gardenDetails.GarderItems)
                {
                    if (gi.CropsId == "0" && gi.Shared == "0" && gi.Status == "1")
                    {
                        string seedName = GetFarmingSeedName(inTimeObject.AccountSetting, gardenDetails.Account.Rank);
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

        private DateTime GetRipeTime(string crops)
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

                    return dt.AddMinutes(1);
                }
            }

            return DateTime.MaxValue;
        }

        private void StealInTime(object o)
        {
            InTimeObject inTimeObject = o as InTimeObject;
        }

        private void AddInTimeObject(InTimeObject inTimeObject)
        {
            lock (inTimeObjectList)
            {
                for (int i = 0; i < inTimeObjectList.Count; i++)
                {
                    if (inTimeObjectList[i].LoginEmail == inTimeObject.LoginEmail)
                    {
                        inTimeObjectList[i].ActiveTime = inTimeObject.ActiveTime;

                        return;
                    }
                }

                inTimeObjectList.Add(inTimeObject);
            }
        }

        private void DeleteInTimeObject(InTimeObject inTimeObject)
        {
            lock (inTimeObjectList)
            {
                for (int i = 0; i < inTimeObjectList.Count; i++)
                {
                    if (inTimeObjectList[i].LoginEmail == inTimeObject.LoginEmail)
                    {
                        inTimeObjectList.RemoveAt(i);

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
            txtInTimeBoard.AppendText(msg);
        }

        class InTimeObject
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
