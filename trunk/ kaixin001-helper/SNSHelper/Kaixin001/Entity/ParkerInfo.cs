using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity
{
    /// <summary>
    /// 泊车者信息
    /// </summary>
    public class ParkerInfo : EntityBase
    {
        public ParkerInfo()
            : base()
        {
        }

        public ParkerInfo(object obj)
            : base(obj)
        {
            vuid = JsonHelper.GetJsonStringValue(jsobj, "vuid");
            uid = JsonHelper.GetJsonStringValue(jsobj, "uid");
            neighbor = JsonHelper.GetJsonStringValue(jsobj, "neighbor");
            isfriend = JsonHelper.GetJsonStringValue(jsobj, "isfriend");
            first_fee_parking = JsonHelper.GetJsonStringValue(jsobj, "first_fee_parking");
            first_free_parking = JsonHelper.GetJsonStringValue(jsobj, "first_free_parking");
            real_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "real_name"));
            ta = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "ta"));
            logo50 = JsonHelper.GetJsonStringValue(jsobj, "logo50");
            logo20 = JsonHelper.GetJsonStringValue(jsobj, "logo20");
            avenue = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "avenue"));
            cash = JsonHelper.GetJsonStringValue(jsobj, "cash");
            scene = JsonHelper.GetJsonStringValue(jsobj, "scene");
            sceneid = JsonHelper.GetJsonStringValue(jsobj, "sceneid");
            showmoneyminute = JsonHelper.GetJsonStringValue(jsobj, "showmoneyminute");
            online2 = JsonHelper.GetJsonStringValue(jsobj, "online2");
        }

        string vuid;
        public string VUID
        {
            get
            {
                return vuid;
            }
        }

        string uid;
        public string UID
        {
            get
            {
                return uid;
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

        string isfriend;
        public string IsFriend
        {
            get
            {
                return isfriend;
            }
        }

        string first_fee_parking;
        public string FirstFeeParking
        {
            get
            {
                return first_fee_parking;
            }
        }

        string first_free_parking;
        public string FirstFreeParking
        {
            get
            {
                return first_free_parking;
            }
        }

        string real_name;
        public string RealName
        {
            get
            {
                return real_name;
            }
        }

        string ta;
        public string Ta
        {
            get
            {
                return ta;
            }
        }

        string logo50;
        public string Logo50
        {
            get
            {
                return logo50;
            }
        }

        string logo20;
        public string Logo20
        {
            get
            {
                return logo20;
            }
        }

        string avenue;
        public string Avenue
        {
            get
            {
                return avenue;
            }
        }

        string cash;
        public string Cash
        {
            get
            {
                return cash;
            }
        }

        string scene;
        public string Scene
        {
            get
            {
                return scene;
            }
        }

        string sceneid;
        public string Sceneid
        {
            get
            {
                return sceneid;
            }
        }

        string showmoneyminute;
        public string Showmoneyminute
        {
            get
            {
                return showmoneyminute;
            }
        }

        string online2;
        public string Online2
        {
            get
            {
                return online2;
            }
        }
    }
}
