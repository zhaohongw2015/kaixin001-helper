using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity
{
    public class ParkResult : EntityBase
    {
        public ParkResult()
            : base()
        {
        }

        public ParkResult(object obj)
            : base(obj)
        {
            cash = JsonHelper.GetJsonStringValue(jsobj, "cash");
            neighbor = JsonHelper.GetJsonStringValue(jsobj, "neighbor");
            park_uid = JsonHelper.GetJsonStringValue(jsobj, "park_uid");
            parkid = JsonHelper.GetJsonStringValue(jsobj, "parkid");
            carid = JsonHelper.GetJsonStringValue(jsobj, "carid");
            moneyminute = JsonHelper.GetJsonStringValue(jsobj, "moneyminute");
            errno = JsonHelper.GetJsonStringValue(jsobj, "errno");
            error = ContentHelper.Unicode2Character(JsonHelper.GetJsonStringValue(jsobj, "error"));
            ctime = JsonHelper.GetJsonStringValue(jsobj, "ctime");
        }

        private string cash;
        public string Cash
        {
            get
            {
                return cash;
            }
        }

        private string neighbor;
        public string Neighbor
        {
            get
            {
                return neighbor;
            }
        }

        private string park_uid;
        public string ParkUId
        {
            get
            {
                return park_uid;
            }
        }

        private string parkid;
        public string ParkId
        {
            get
            {
                return parkid;
            }
        }

        private string carid;
        public string CarId
        {
            get
            {
                return carid;
            }
        }

        private string moneyminute;
        public string MoneyMinute
        {
            get
            {
                return moneyminute;
            }
        }

        private string errno;
        public string ErrNo
        {
            get
            {
                return errno;
            }
        }

        private string error;
        public string Error
        {
            get
            {
                return ContentHelper.Unicode2Character(error);
            }
        }

        private string ctime;
        public string CTime
        {
            get
            {
                return ctime;
            }
        }
    }
}
