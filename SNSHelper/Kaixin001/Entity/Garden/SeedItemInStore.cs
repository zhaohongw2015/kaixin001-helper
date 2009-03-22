using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class SeedItemInStore
    {
        public SeedItemInStore()
        {
        }

        public SeedItemInStore(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                errMsg = "错误编号：007．" + e.Message;
                return;
            }

            foreach (XmlNode item in doc.DocumentElement.ChildNodes)
            {
                switch (item.Name.ToLower())
                {
                    case "ret":
                        ret = item.InnerText;
                        break;
                    case "name":
                        name = item.InnerText;
                        break;
                    case "userrank":
                        userRank = item.InnerText;
                        break;
                    case "rank":
                        rank = item.InnerText;
                        break;
                    case "mhours":
                        mhours = item.InnerText;
                        break;
                    case "price":
                        price = item.InnerText;
                        break;
                    case "antistealdays":
                        antiStealDays = item.InnerText;
                        break;
                    case "fruitprice":
                        fruitPrice = item.InnerText;
                        break;
                    case "nocash":
                        noCash = item.InnerText;
                        break;
                    case "fruitpic":
                        fruitPic = item.InnerText;
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

        private string name = string.Empty;
        /// <summary>
        /// 种子名
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string userRank = string.Empty;
        /// <summary>
        /// 用户等级
        /// </summary>
        public string UserRank
        {
            get
            {
                return userRank;
            }
            set
            {
                userRank = value;
            }
        }

        private string rank = string.Empty;
        /// <summary>
        /// 种子等级
        /// </summary>
        public string Rank
        {
            get
            {
                return rank;
            }
            set
            {
                rank = value;
            }
        }

        private string mhours = string.Empty;
        /// <summary>
        /// 成熟时间
        /// </summary>
        public string MHours
        {
            get
            {
                return mhours;
            }
            set
            {
                mhours = value;
            }
        }

        private string price = string.Empty;
        /// <summary>
        /// 种子价格
        /// </summary>
        public string Price
        {
            get
            {
                return price;
            }
            set
            {
                price = value;
            }
        }

        private string antiStealDays = string.Empty;
        /// <summary>
        /// 防偷保护期
        /// </summary>
        public string AntiStealDays
        {
            get
            {
                return antiStealDays;
            }
            set
            {
                antiStealDays = value;
            }
        }

        private string fruitPrice = string.Empty;
        /// <summary>
        /// 果实单价
        /// </summary>
        public string FruitPrice
        {
            get
            {
                return fruitPrice;
            }
            set
            {
                fruitPrice = value;
            }
        }

        private string noCash = string.Empty;
        /// <summary>
        /// ？？？
        /// </summary>
        public string NoCash
        {
            get
            {
                return noCash;
            }
            set
            {
                noCash = value;
            }
        }

        private string fruitPic = string.Empty;
        public string FruitPic
        {
            get
            {
                return fruitPrice;
            }
            set
            {
                fruitPrice = value;
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
