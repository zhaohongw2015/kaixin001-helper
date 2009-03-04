using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity.Parking
{
    /// <summary>
    /// 玩家车辆停车情况
    /// </summary>
    public class CarInfo : EntityBase
    {
        public CarInfo()
            : base()
        {
        }

        public CarInfo(object obj)
            : base(obj)
        {
            carid = JsonHelper.GetJsonStringValue(jsobj, "carid");
            price = JsonHelper.GetJsonStringValue(jsobj, "price");
            neighbor = JsonHelper.GetJsonStringValue(jsobj, "neighbor");
            park_uid = JsonHelper.GetJsonStringValue(jsobj, "park_uid");
            parkid = JsonHelper.GetJsonStringValue(jsobj, "parkid");
            ctime = JsonHelper.GetJsonStringValue(jsobj, "ctime");
            car_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "car_name"));
            car_logo_big = JsonHelper.GetJsonStringValue(jsobj, "car_logo_big");
            car_logo_small = JsonHelper.GetJsonStringValue(jsobj, "car_logo_small");
            car_logo_flash = JsonHelper.GetJsonStringValue(jsobj, "car_logo_flash");
            car_police = JsonHelper.GetJsonStringValue(jsobj, "car_police");
            car_trademark = JsonHelper.GetJsonStringValue(jsobj, "car_trademark");
            park_real_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "park_real_name"));
            park_logo20 = JsonHelper.GetJsonStringValue(jsobj, "park_logo20");
            park_profit = JsonHelper.GetJsonStringValue(jsobj, "park_profit");
            park_moneyminute = JsonHelper.GetJsonStringValue(jsobj, "park_moneyminute");
            car_trademarkname = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "car_trademarkname"));
        }

        string carid;
        /// <summary>
        /// 汽车编号
        /// </summary>
        public string CarId
        {
            get
            {
                return carid;
            }
        }

        string price;
        /// <summary>
        /// 单价
        /// </summary>
        public string Price
        {
            get
            {
                return price;
            }
        }

        string neighbor;
        public string Neighbor
        {
            get
            {
                return neighbor;
            }
        }

        string park_uid;
        public string ParkUId
        {
            get
            {
                return park_uid;
            }
        }

        string parkid;
        public string ParkId
        {
            get
            {
                return parkid;
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

        string car_name;
        public string CarName
        {
            get
            {
                return car_name;
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
        public string CarPolice
        {
            get
            {
                return car_police;
            }
        }

        string car_trademark;
        public string CarTrademark
        {
            get
            {
                return car_trademark;
            }
        }

        string car_trademarkname;
        public string CarTrademarkName
        {
            get
            {
                return car_trademarkname;
            }
        }

        string park_real_name;
        public string ParkRealName
        {
            get
            {
                return park_real_name;
            }
        }

        string park_logo20;
        public string ParkLogo20
        {
            get
            {
                return park_logo20;
            }
        }

        string park_profit;
        public string ParkProfit
        {
            get
            {
                return park_profit;
            }
        }

        string park_moneyminute;
        public string ParkMoneyMinute
        {
            get
            {
                return park_moneyminute;
            }
        }

        string exceptParkUID = string.Empty;
        public string ExceptParkUID
        {
            get
            {
                return exceptParkUID;
            }
            set
            {
                exceptParkUID = value;
            }
        }
    }
}
