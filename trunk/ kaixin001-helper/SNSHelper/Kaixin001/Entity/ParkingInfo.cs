using System;
using System.Collections.Generic;
using System.Text;
using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity
{
    /// <summary>
    /// 玩家私家车位停车情况
    /// </summary>
    public class ParkingInfo : EntityBase
    {
        public ParkingInfo()
            : base()
        {
        }

        public ParkingInfo(object obj)
            : base(obj)
        {
            parkid = JsonHelper.GetJsonStringValue(jsobj, "parkid");
            car_uid = JsonHelper.GetJsonStringValue(jsobj, "car_uid");
            carid = JsonHelper.GetJsonStringValue(jsobj, "carid");
            ctime = JsonHelper.GetJsonStringValue(jsobj, "ctime");
            car_real_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "car_real_name"));
            car_logo20 = JsonHelper.GetJsonStringValue(jsobj, "car_logo20");
            car_isfriend = JsonHelper.GetJsonStringValue(jsobj, "car_isfriend");
            car_logo_big = JsonHelper.GetJsonStringValue(jsobj, "car_logo_big");
            car_logo_small = JsonHelper.GetJsonStringValue(jsobj, "car_logo_small");
            car_logo_flash = JsonHelper.GetJsonStringValue(jsobj, "car_logo_flash");
            car_police =JsonHelper.GetJsonStringValue(jsobj, "car_police");
            car_profit = JsonHelper.GetJsonStringValue(jsobj, "car_profit");
            car_trademark = JsonHelper.GetJsonStringValue(jsobj, "car_trademark");
            car_trademarkname = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "car_trademarkname"));
            car_price = JsonHelper.GetJsonStringValue(jsobj, "car_price");
            car_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "car_name"));
        }

        string parkid;
        public string ParkId
        {
            get
            {
                return parkid;
            }
        }

        string car_uid;
        public string CarUId
        {
            get
            {
                return car_uid;
            }
        }

        string carid;
        public string CarId
        {
            get
            {
                return carid;
            }
            set
            {
                carid = value;
            }
        }

        string ctime;
        public string CTime
        {
            get
            {
                return ctime;
            }
        }

        string car_real_name;
        /// <summary>
        /// 停在该车位汽车主人的名字
        /// </summary>
        public string CarRealName
        {
            get
            {
                return car_real_name;
            }
        }

        string car_logo20;
        public string CarLogo20
        {
            get
            {
                return car_logo20;
            }
        }

        string car_isfriend;
        public string CarIsfriend
        {
            get
            {
                return car_isfriend;
            }
        }

        string car_logo_big;
        public string CarLogoBig
        {
            get
            {
                return car_logo_big;
            }
        }

        string car_logo_small;
        public string CarLogoSmall
        {
            get
            {
                return car_logo_small;
            }
        }

        string car_logo_flash;
        public string CarLogoFlash
        {
            get
            {
                return car_logo_flash;
            }
        }

        string car_police;
        /// <summary>
        /// 标识停在该车位上的车是否是冒牌警车
        /// </summary>
        public string CarPolice
        {
            get
            {
                return car_police;
            }
        }

        string car_profit;
        /// <summary>
        /// 车位利润
        /// </summary>
        public string CarProfit
        {
            get
            {
                return car_profit;
            }
        }

        string car_trademark;
        /// <summary>
        /// 停在该车位上汽车的商标图路径
        /// </summary>
        public string CarTrademark
        {
            get
            {
                return car_trademark;
            }
        }

        string car_trademarkname;
        /// <summary>
        /// 停在该车位上汽车的商标名
        /// </summary>
        public string CarTrademarkName
        {
            get
            {
                return car_trademarkname;
            }
        }

        string car_price;
        /// <summary>
        /// 停在该车位上的汽车单价
        /// </summary>
        public string CarPrice
        {
            get
            {
                return car_price;
            }
        }

        string car_name;
        /// <summary>
        /// 停在该车位上的汽车名
        /// </summary>
        public string CarName
        {
            get
            {
                return car_name;
            }
        }

        /// <summary>
        /// 是否为免费车位
        /// </summary>
        public bool IsParkFree
        {
            get
            {
                return Convert.ToBoolean(Convert.ToInt32(ParkId) >> 16 & 255);
            }
        }
    }
}
