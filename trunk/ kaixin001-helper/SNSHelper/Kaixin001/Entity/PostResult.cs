using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity
{
    /// <summary>
    /// 贴条结果
    /// </summary>
    public class PostResult : EntityBase
    {
        public PostResult()
            : base()
        {
        }

        public PostResult(object o)
            : base(o)
        {
            cash = JsonHelper.GetJsonStringValue(jsobj, "cash");
            parkid = JsonHelper.GetJsonStringValue(jsobj, "parkid");
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

        private string parkid;
        public string ParkId
        {
            get
            {
                return parkid;
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
                return error;
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
