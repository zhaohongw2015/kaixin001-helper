using System;
using System.Collections.Generic;
using System.Text;
using SNSHelper.Kaixin001.Entity;
using SNSHelper_Win.Helper;
using SNSHelper_Win.Entity;
using SNSHelper.Kaixin001;

namespace SNSHelper_Win
{
    public partial class frmMain
    {
        private List<string> parkedCarIDList = new List<string>();  // 已停好车的车辆ID列表
        private ParkResult currentParkResult = new ParkResult();
        private string lastParkedUID = string.Empty;
        private List<string> parkedFailedMsg = new List<string>();
        double cashBeforeParking = 0;

        private void ParkCars(ParkingHelper helper)
        {
            cashBeforeParking = Convert.ToDouble(helper.Parker.Cash);

            // 处理网络上新的好友
            DealWithNewNetFriend(helper.ParkerFriendList);

            // 重设可停车的好友
            ResetParkerFriend(helper);

            // 排除不需要停车的车辆
            ExceptNonNeedParkCar(helper.CarList);

        Step1:
            // 是否所有车都停好(若停好的车辆数和需要停到车辆数一样，说明所有车都已停好)
            if (helper.CarList.Count == parkedCarIDList.Count)
            {
                // End
                return;
            }

            // 是否所有好友车库都已停满
            if (IsAllFriendGarageFull(helper.ParkerFriendList))
            {
                // End
                if (helper.CarList.Count != parkedCarIDList.Count)
                {
                    ShowParkingMessage("停车失败！");
                    parkedFailedMsg.Add(string.Format("{0} 停车失败！失败的原因可能是：根据配置文件，所有能停车的好友车位都已停满！", currentAccount.LoginEmail));

                    return;
                }

                return;
            }

            ParkerFriendInfo currentGarage = GetValidGarage(helper.ParkerFriendList);

            List<ParkingInfo> currentParkList = helper.GetFirendParkingInfo(currentGarage.UId);

        Step2:
            ParkingInfo currentParking = GetValidPark(currentParkList);
            if (currentParking == null)
            {
                currentGarage.Full = "1";
                goto Step1;
            }

        Step3:
            CarInfo currentCar = GetValidCar(helper.CarList, currentGarage);

            if (currentCar != null)
            {
                lastParkedUID = currentCar.ParkUId;
                currentParkResult = helper.ParkOneCar(currentGarage, currentCar, currentParking);

                if (currentParkResult != null)
                {
                    if (currentParkResult.ErrNo.Equals("0"))
                    {
                        currentParking.CarId = currentCar.CarId;
                        parkedCarIDList.Add(currentCar.CarId);
                        helper.SetParkNoFull(lastParkedUID);
                        cashBeforeParking += Convert.ToDouble(currentParkResult.Cash);
                        ShowParkingMessage(string.Format("{0}，被成功停至 {1} 的私家车位，你获得{2}元停车收入！", currentCar.CarName, currentGarage.RealName, currentParkResult.Cash));

                        if ((currentParking = GetValidPark(currentParkList)) != null)
                        {
                            goto Step3;
                        }
                        else
                        {
                            currentGarage.Full = "1";
                            goto Step1;
                        }
                    }
                    else
                    {
                        if (currentParkResult.Error.ToString().Equals("一辆车不能连续两次停放在同一个人的车位"))
                        {
                            currentCar.ExceptParkUID = currentGarage.UId;
                        }
                        goto Step3;
                    }
                }
            }

            currentGarage.Full = "1";
            goto Step1;
        }

        private void ResetParkerFriend(ParkingHelper helper)
        {
            List<ParkerFriendInfo> temp = new List<ParkerFriendInfo>();
            for (int i = 0; i < currentAccount.CanParkFriendList.Count; i++)
            {
                for (int j = 0; j < helper.ParkerFriendList.Count; j++)
                {
                    if (helper.ParkerFriendList[j].UId.Equals(currentAccount.CanParkFriendList[i].UID))
                    {
                        temp.Add(helper.ParkerFriendList[j]);
                        break;
                    }
                }
            }

            helper.ParkerFriendList = temp;
        }

        /// <summary>
        /// 处理网络上新的好友
        /// </summary>
        /// <param name="helper"></param>
        private void DealWithNewNetFriend(List<ParkerFriendInfo> netFriendList)
        {
            List<ParkerFriendInfo> newNetFriendList = GetNewNetFriend(netFriendList, currentAccount.FriendSettings);

            if (newNetFriendList != null && newNetFriendList.Count != 0)
            {
                AddNewNetFriend(newNetFriendList, currentAccount);
            }
        }

        /// <summary>
        /// 把网络上新的好友信息添加到本地系统中
        /// </summary>
        /// <param name="newNetFriendList">网或上新的好友列表</param>
        /// <param name="accountSetting">帐号设置</param>
        private void AddNewNetFriend(List<ParkerFriendInfo> newNetFriendList, AccountSetting accountSetting)
        {
            FriendSetting temp;
            for (int i = 0; i < newNetFriendList.Count; i++)
            {
                temp = new FriendSetting(newNetFriendList[i].RealName, newNetFriendList[i].UId);
                accountSetting.FriendSettings.Add(temp);
                accountSetting.CanParkFriendList.Add(temp);
            }

            ConfigHelper.SaveConfig();
        }

        /// <summary>
        /// 指定好友(parkUID)是否已存在本地好友列表中
        /// </summary>
        /// <param name="localFriendList">本地好友列表</param>
        /// <param name="parkUID">好友ID</param>
        /// <returns></returns>
        private bool IsIncludeInLocal(List<FriendSetting> localFriendList, string parkUID)
        {
            for (int i = 0; i < localFriendList.Count; i++)
            {
                if (localFriendList[i].UID.Equals(parkUID))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 排除不需要进行停车操作的车辆
        /// </summary>
        /// <param name="helper"></param>
        private void ExceptNonNeedParkCar(List<CarInfo> carList)
        {
            parkedCarIDList = new List<string>();
            List<string> nonNeedPark = new List<string>();

            for (int i = 0; i < carList.Count; i++)
            {
                if (!string.IsNullOrEmpty(carList[i].ParkMoneyMinute) && Convert.ToInt32(carList[i].ParkProfit) <= Convert.ToInt32(currentAccount.MinPark))
                {
                    nonNeedPark.Add(string.Format("{0} 停车利润还不到{1}，无需停车！", carList[i].CarName, currentAccount.MinPark));
                    parkedCarIDList.Add(carList[i].CarId);
                }
            }

            ShowParkingMessage(nonNeedPark);
        }

        /// <summary>
        /// 是否好友的车库都停满了
        /// </summary>
        /// <returns></returns>
        private bool IsAllFriendGarageFull(List<ParkerFriendInfo> friendGarageList)
        {
            for (int i = 0; i < friendGarageList.Count; i++)
            {
                if (friendGarageList[i].Full.Equals("0"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取一个还有空位的车库
        /// </summary>
        /// <returns></returns>
        private ParkerFriendInfo GetValidGarage(List<ParkerFriendInfo> friendGarageList)
        {
            for (int i = 0; i < friendGarageList.Count; i++)
            {
                if (friendGarageList[i].Full.Equals("0"))
                {
                    return friendGarageList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 获取车库中的一个空车位
        /// </summary>
        /// <returns></returns>
        private ParkingInfo GetValidPark(List<ParkingInfo> currentParkList)
        {
            for (int i = 0; i < currentParkList.Count; i++)
            {
                //若私家车位不是免费车位，且私家车位上没有停车
                if (!currentParkList[i].IsParkFree && currentParkList[i].CarId.Equals("0"))
                {
                    return currentParkList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 获取一辆等待停车的车辆
        /// </summary>
        /// <returns></returns>
        private CarInfo GetValidCar(List<CarInfo> carList, ParkerFriendInfo currentGarage)
        {
            for (int i = 0; i < carList.Count; i++)
            {
                if (!parkedCarIDList.Contains(carList[i].CarId))
                {
                    if (!carList[i].ParkUId.Equals(currentGarage.UId) && !carList[i].ExceptParkUID.Equals(currentGarage.UId))
                    {
                        return carList[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取网络上新的好友信息
        /// </summary>
        /// <param name="netFriendList">网络好友列表</param>
        /// <param name="localFriendList">本地好友列表</param>
        /// <returns></returns>
        private List<ParkerFriendInfo> GetNewNetFriend(List<ParkerFriendInfo> netFriendList, List<FriendSetting> localFriendList)
        {
            List<ParkerFriendInfo> newNetFriendList = new List<ParkerFriendInfo>();
            for (int i = 0; i < netFriendList.Count; i++)
            {
                if (!IsIncludeInLocal(localFriendList, netFriendList[i].UId))
                {
                    ShowParkingMessage(string.Format("{0} 可能是你的新好友，系统正为你添加配置信息！", netFriendList[i].RealName));
                    newNetFriendList.Add(netFriendList[i]);
                }
            }

            return newNetFriendList;
        }
    }
}
