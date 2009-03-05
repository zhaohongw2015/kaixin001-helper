using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class HavestResult
    {
        public HavestResult()
        {
        }

        public HavestResult(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"gb2312\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                errMsg = "错误编号：003．" + e.Message;
                return;
            }

            LoadHavestResult(doc.DocumentElement);
        }

        private void LoadHavestResult(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "leftnum":
                        leftNum = item.InnerText;
                        break;
                    case "stealnum":
                        stealNum = item.InnerText;
                        break;
                    case "num":
                        num = item.InnerText;
                        break;
                    case "seedname":
                        seedName = item.InnerText;
                        break;
                    case "fruitpic":
                        fruitPic = item.InnerText;
                        break;
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

        private string leftNum = string.Empty;
        /// <summary>
        /// 剩下的果实数
        /// </summary>
        public string LeftNum
        {
            get
            {
                return leftNum;
            }
            set
            {
                leftNum = value;
            }
        }

        private string stealNum = string.Empty;
        /// <summary>
        /// 偷盗的果实说
        /// </summary>
        public string StealNum
        {
            get
            {
                return stealNum;
            }
            set
            {
                stealNum = value;
            }
        }

        private string num = string.Empty;
        /// <summary>
        /// 本次操作有效个数
        /// </summary>
        public string Num
        {
            get
            {
                return num;
            }
            set
            {
                num = value;
            }
        }

        private string seedName = string.Empty;
        /// <summary>
        /// 果实名
        /// </summary>
        public string SeedName
        {
            get
            {
                return seedName;
            }
            set
            {
                seedName = value;
            }
        }

        private string fruitPic = string.Empty;
        /// <summary>
        /// 果实图片路径
        /// </summary>
        public string FruitPic
        {
            get
            {
                return fruitPic;
            }
            set
            {
                fruitPic = value;
            }
        }

        private string ret = string.Empty;
        /// <summary>
        /// 结果
        /// </summary>
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
        /// <summary>
        /// 原因
        /// </summary>
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
