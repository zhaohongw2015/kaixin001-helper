using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class FarmResult
    {
        public FarmResult(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"gb2312\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                errMsg = "错误编号：002．" + e.Message;
                return;
            }

            LoadFarmResult(doc.DocumentElement);
        }

        private void LoadFarmResult(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ret":
                        ret = item.InnerText;
                        break;
                    case "reason":
                        reason = item.InnerText;
                        break;
                    default:
                        break;
                }
            }

        }
        private string ret = string.Empty;
        public string Ret
        {
            get
            {
                return ret;
            }
            set
            {
                ret = value;
            }
        }

        private string reason = string.Empty;
        public string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

        private string errMsg = string.Empty;
        public string ErrMsg
        {
            get
            {
                return errMsg;
            }
            set
            {
                errMsg = value;
            }
        }

    }
}
