using System;
using System.Collections.Generic;
using SNSHelper.Common;
using SNSHelper.Kaixin001.Entity;
using SNSHelper.Kaixin001.Entity.Parking;
using SNSHelper.Kaixin001.Enum;

namespace SNSHelper.Kaixin001
{
    public class ParkingHelper : AppHelperBase
    {
        #region Fields

        bool isResetData = false;

        #endregion

        #region Properties

        private string verify;
        /// <summary>
        /// verify
        /// </summary>
        public string Verify
        {
            get
            {
                return verify;
            }
        }

        private string acc;
        /// <summary>
        /// acc
        /// </summary>
        public string Acc
        {
            get
            {
                return acc;
            }
        }

        private ParkerInfo parkerInfo;
        /// <summary>
        /// 玩家信息
        /// </summary>
        public ParkerInfo Parker
        {
            get
            {
                return parkerInfo;
            }
            set
            {
                parkerInfo = value;
            }
        }

        private List<CarInfo> carInfoList;
        /// <summary>
        /// 玩家车辆停车情况
        /// </summary>
        public List<CarInfo> CarList
        {
            get
            {
                return carInfoList;
            }
            set
            {
                carInfoList = value;
            }
        }

        private List<ParkingInfo> parkingInfoList;
        /// <summary>
        /// 玩家私家车位停车情况
        /// </summary>
        public List<ParkingInfo> ParkingList
        {
            get
            {
                return parkingInfoList;
            }
            set
            {
                parkingInfoList = value;
            }
        }

        private List<ParkerFriendInfo> parkerFriendInfoList;
        /// <summary>
        /// 玩家好友信息
        /// </summary>
        public List<ParkerFriendInfo> ParkerFriendList
        {
            get
            {
                return parkerFriendInfoList;
            }
            set
            {
                parkerFriendInfoList = value;
            }
        }

        #endregion

        #region Constructor
        public ParkingHelper()
        {
            AppID = "1040";

            GetParkerDetails();
        }
        #endregion

        #region Common

        /// <summary>
        /// 获取acc
        /// </summary>
        /// <param name="parkingHtml">争车位页面HTML代码</param>
        /// <returns></returns>
        private string GetAcc(string parkingHtml)
        {
            string[] temp = parkingHtml.Split(new string[] { "acc()" }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 0)
            {
                throw new Exception("找不到acc()方法");
            }

            string js = temp[0].Substring(temp[0].LastIndexOf('}') + 1) + " acc() " + temp[1].Substring(0, temp[1].IndexOf("function")) + " acc();";

            return JSHelper.EvalJavascript(js);
        }

        /// <summary>
        /// 获取泊车者相关信息：泊车者信息、自家车位停车情况、自己车辆的停车情况
        /// </summary>
        private void GetParkerDetails()
        {
            if (!Utility.IsLogin)
            {
                throw new Exception("请先登录！");
            }

            parkerInfo = new ParkerInfo();
            carInfoList = new List<CarInfo>();
            parkingInfoList = new List<ParkingInfo>();
            parkerFriendInfoList = new List<ParkerFriendInfo>();
            verify = string.Empty;
            acc = string.Empty;

            string parkingHTML = new HttpHelper().GetHtml(AppUrl + AppID, Utility.Cookies);

            if (!parkingHTML.Contains("争车位"))
            {
                throw new Exception("读取争车位信息出错");
            }

            string userData = ContentHelper.GetMidString(parkingHTML, "v_userdata = ", "\n");

            if (string.IsNullOrEmpty(userData))
            {
                throw new Exception("读取争车位信息出错");
            }

            userData = JsonHelper.InitJsonString(userData.Trim());

            object o = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(userData);
            Newtonsoft.Json.JavaScriptObject jo = o as Newtonsoft.Json.JavaScriptObject;

            parkerInfo = new ParkerInfo(jo["user"]);
            string str = parkerInfo.RealName;

            Newtonsoft.Json.JavaScriptArray jsa = jo["parking"] as Newtonsoft.Json.JavaScriptArray;
            for (int i = 0; i < jsa.Count; i++)
            {
                parkingInfoList.Add(new ParkingInfo(jsa[i]));
            }

            jsa = jo["car"] as Newtonsoft.Json.JavaScriptArray;
            for (int i = 0; i < jsa.Count; i++)
            {
                carInfoList.Add(new CarInfo(jsa[i]));
            }

            string friendData = ContentHelper.GetMidString(parkingHTML, "var v_frienddata = ", "\n"); // 获取v_frienddata变量
            friendData = JsonHelper.InitJsonString(friendData.Trim());
            o = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(friendData);
            Newtonsoft.Json.JavaScriptArray jsaFriend = o as Newtonsoft.Json.JavaScriptArray;
            for (int i = 0; i < jsaFriend.Count; i++)
            {
                parkerFriendInfoList.Add(new ParkerFriendInfo(jsaFriend[i]));
            }

            verify = ContentHelper.GetMidString(parkingHTML, "var g_verify = \"", "\";"); // 获取verify变量
            acc = GetAcc(parkingHTML);
        }

        /// <summary>
        /// 获取好友私家车位停车情况
        /// </summary>
        /// <param name="friendUId">好友编号</param>
        /// <returns></returns>
        public List<ParkingInfo> GetFirendParkingInfo(string friendUId)
        {
            string url;
            if (friendUId == "1" || friendUId == "2")
            {
                url = "http://www.kaixin001.com/parking/neighbor.php";
            }
            else
            {
                url = "http://www.kaixin001.com/parking/user.php";
            }

            string postParams = string.Format("puid={0}&verify={1}&_=", friendUId, verify);

            string friendParkingJSON = new HttpHelper().GetHtml(url, postParams, true, Utility.Cookies);

            if (string.IsNullOrEmpty(friendParkingJSON))
            {
                return null;
            }

            friendParkingJSON = JsonHelper.InitJsonString(friendParkingJSON);

            List<ParkingInfo> friendParkingInfoList = new List<ParkingInfo>();

            object o = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(friendParkingJSON);
            Newtonsoft.Json.JavaScriptObject jo = o as Newtonsoft.Json.JavaScriptObject;
            Newtonsoft.Json.JavaScriptArray jsa = jo["parking"] as Newtonsoft.Json.JavaScriptArray;
            for (int i = 0; i < jsa.Count; i++)
            {
                friendParkingInfoList.Add(new ParkingInfo(jsa[i]));
            }

            return friendParkingInfoList;
        }

        #endregion

        #region 停车的相关方法

        /// <summary>
        /// 停一辆车
        /// </summary>
        /// <param name="parkerFriendInfo">玩家好友（汽车将停在该好友的私家车位上）</param>
        /// <param name="carInfo">汽车（将进行的汽车）</param>
        /// <param name="parkingInfo">私家车位（汽车将停在该私家车位上）</param>
        public ParkResult ParkOneCar(ParkerFriendInfo parkerFriendInfo, CarInfo carInfo, ParkingInfo parkingInfo)
        {
            string parkUrl = "http://www.kaixin001.com/parking/park.php";
            string postParams = string.Format("_=&acc={0}&carid={1}&first_fee_parking={2}&neighbor={3}&park_uid={4}&parkid={5}&verify={6}", acc, carInfo.CarId, parkerInfo.FirstFeeParking, parkerFriendInfo.Neighbor, parkerFriendInfo.UId, parkingInfo.ParkId, verify);

            HttpHelper helper = new HttpHelper();
            string parkResultHtml = helper.GetHtml(parkUrl, postParams, true, Utility.Cookies);
            parkResultHtml = JsonHelper.InitJsonString(parkResultHtml);

            if (string.IsNullOrEmpty(parkResultHtml))
            {
                return null;
            }

            object o = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(parkResultHtml);
            return new ParkResult(o);
        }

        /// <summary>
        /// 把好友车库标识为有空闲车位
        /// </summary>
        /// <param name="parkID">好友车库ID</param>
        public void SetParkNoFull(string parkID)
        {
            for (int i = 0; i < parkerFriendInfoList.Count; i++)
            {
                if (parkerFriendInfoList[i].UId.Equals(parkID))
                {
                    parkerFriendInfoList[i].Full = "0";
                    return;
                }
            }
        }

        #endregion

        #region 贴条的相关方法

        /// <summary>
        /// 对指定车位进行贴条
        /// </summary>
        /// <param name="parkingInfo">将进行贴条的车位信息</param>
        public PostResult PostOneCar(ParkingInfo parkingInfo)
        {
            // 该车位上未停车
            if (parkingInfo.CarId.Equals("0"))
            {
                return null;
            }

            // 车位利润不够贴条
            if (!string.IsNullOrEmpty(parkingInfo.CarProfit) && Convert.ToInt32(parkingInfo.CarProfit) <= 150)
            {
                return null;
            }

            string postUrl = "http://www.kaixin001.com/parking/post.php";
            string postParams = string.Format("_=&acc={0}&parkid={1}&verify={2}", acc, parkingInfo.ParkId, verify);

            string postResultJson = new HttpHelper().GetHtml(postUrl, postParams, true, Utility.Cookies);

            if (string.IsNullOrEmpty(postResultJson))
            {
                return null;
            }

            object o = Newtonsoft.Json.JavaScriptConvert.DeserializeObject(JsonHelper.InitJsonString(postResultJson));

            return new PostResult(o);
        }

        #endregion

        #region 买车、买道具的相关方法

        /// <summary>
        /// 购买汽车
        /// </summary>
        /// <param name="carID">汽车编号</param>
        /// <param name="color">颜色</param>
        /// <returns></returns>
        public bool BuyCar(string carID, CarColor color)
        {
            HttpHelper httpHelper = new HttpHelper(Utility.Cookies);
            string str = httpHelper.GetHtml("http://www.kaixin001.com/parking/purchase.php", string.Format(" verify={0}&action=1&carid={1}&color={2}&_=", verify, carID, GetColorNumber(color)), true, Utility.Cookies);
            str = httpHelper.GetHtml(string.Format("http://www.kaixin001.com/parking/purchase.php?verify={0}&action=1&carid={1}&color={2}", verify, carID, GetColorNumber(color)), Utility.Cookies);

            str = ContentHelper.GetMidString(str, "display:block", "</div>");

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return str.IndexOf("你购买了") > -1;
        }
        
        /// <summary>
        /// 获取颜色编号
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns></returns>
        private string GetColorNumber(CarColor color)
        {
            switch (color)
            {
                case CarColor.None:
                    return string.Empty;
                case CarColor.White:
                    return "16777215";
                case CarColor.Silver:
                    return "13421772";
                case CarColor.Black:
                    return "0";
                case CarColor.Red:
                    return "16711680";
                case CarColor.Blue:
                    return "255";
                case CarColor.Yellow:
                    return "16776960";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 购买指定编号的道具
        /// </summary>
        /// <param name="cardId">道具编号</param>
        /// <returns></returns>
        public bool BuyCard(string cardId)
        {
            HttpHelper httpHelper = new HttpHelper(Utility.Cookies);
            string str = httpHelper.GetHtml("http://www.kaixin001.com/parking/buycard.php", string.Format("verify={0}&cardid={1}", verify, cardId), true, Utility.Cookies);

            str = httpHelper.GetHtml(string.Format("http://www.kaixin001.com/parking/buycard.php?verify={0}&action=1&cardid={1}", verify, cardId), Utility.Cookies);
            str = ContentHelper.GetMidString(str, "display:block", "</div>");

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return str.IndexOf("成功") > -1;
        }

        /// <summary>
        /// 判断是否已有指定编号的道具
        /// </summary>
        /// <param name="cardId">道具编号</param>
        public bool IsCardExsit(string cardId)
        {
            HttpHelper httpHelper = new HttpHelper(Utility.Cookies);
            string str = httpHelper.GetHtml("http://www.kaixin001.com/app/app.php?aid=1040&url=mycard.php", Utility.Cookies);

            return str.IndexOf(string.Format("usecard('{0}', '0', '0')", cardId)) > -1;
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="cardId">道具编号</param>
        /// <returns></returns>
        public bool UseCard(string cardId)
        {
            HttpHelper httpHelper = new HttpHelper(Utility.Cookies);
            string html = string.Empty;
            switch (cardId)
            {
                case "15":
                    html = httpHelper.GetHtml("http://www.kaixin001.com/parking/card_resetpark.php", string.Format("verify={0}&cardid={1}", verify, cardId), true, Utility.Cookies);
                    html = httpHelper.GetHtml(string.Format("http://www.kaixin001.com/parking/card_resetpark.php?verify={0}&cardid=15&action=1", verify), Utility.Cookies);
                    html = ContentHelper.GetMidString(html, "display:block", "</div>");
                    return html.IndexOf("成功") > -1;
                default:
                    break;
            }

            return false;
        }
        #endregion
    }
}
