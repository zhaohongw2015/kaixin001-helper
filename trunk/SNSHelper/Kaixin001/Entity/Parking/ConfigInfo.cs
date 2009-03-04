using SNSHelper.Common;

namespace SNSHelper.Kaixin001.Entity.Parking
{
    public class ConfigInfo : EntityBase
    {
        public ConfigInfo()
            : base()
        {
        }

        public ConfigInfo(object obj)
            : base(obj)
        {
            money_minute = JsonHelper.GetJsonStringValue(jsobj, "money_minute");
            money_max = JsonHelper.GetJsonStringValue(jsobj, "money_max");
        }

        string money_minute;
        public string MoneyMinute
        {
            get
            {
                return money_minute;
            }
        }

        string money_max;
        public string MoneyMax
        {
            get
            {
                return money_max;
            }
        }
    }
}
