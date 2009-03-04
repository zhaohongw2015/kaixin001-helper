using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity.Parking
{
    public class ParkerFriendInfo : EntityBase
    {
        public ParkerFriendInfo()
            : base()
        {
        }

        public ParkerFriendInfo(object obj)
            : base(obj)
        {
            uid = JsonHelper.GetJsonStringValue(jsobj, "uid");
            real_name = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "real_name"));
            ta = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "ta"));
            full = JsonHelper.GetJsonStringValue(jsobj, "full");
            scenemoney = JsonHelper.GetJsonStringValue(jsobj, "scenemoney");
            scenename = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "scenename"));
            neighbor = JsonHelper.GetJsonStringValue(jsobj, "neighbor");
            online = JsonHelper.GetJsonStringValue(jsobj, "online");
        }

        string uid;
        public string UId
        {
            get
            {
                return uid;
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

        string full;
        /// <summary>
        /// 0： 好友私家车位为满； 1：好友私家车位已满；2：好友私家车位除免费停车位外已满
        /// </summary>
        public string Full
        {
            get
            {
                return full;
            }
            set
            {
                full = value;
            }
        }

        string scenemoney;
        public string SceneMoney
        {
            get
            {
                return scenemoney;
            }
        }

        string scenename;
        public string SceneName
        {
            get
            {
                return scenename;
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

        string online;
        public string Online
        {
            get
            {
                return online;
            }
        }
    }
}
