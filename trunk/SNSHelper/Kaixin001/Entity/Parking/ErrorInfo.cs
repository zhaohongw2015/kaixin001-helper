using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity.Parking
{
    public class ErrorInfo : EntityBase
    {
        public ErrorInfo()
            : base()
        {
        }

        public ErrorInfo(object obj)
            : base(obj)
        {
        }

        public string Error
        {
            get
            {
                return JsonHelper.GetJsonStringValue(jsobj, "error");
            }
        }
    }
}
