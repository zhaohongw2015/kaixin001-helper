using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class SeedData
    {
        public SeedData()
        {
        }

        public SeedData(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                errMsg = "错误编号：005．" + e.Message;
                return;
            }

            if (doc.DocumentElement.SelectSingleNode("seed") != null)
            {
                LoadSeedItems(doc.DocumentElement.SelectSingleNode("seed"));
            }

            if (doc.DocumentElement.SelectSingleNode("ret") != null)
            {
                ret = doc.DocumentElement.SelectSingleNode("ret").InnerText;
            }
        }

        #region 属性

        private List<SeedItem> seedItems = new List<SeedItem>();
        public List<SeedItem> SeedItems
        {
            get
            {
                return seedItems;
            }
            set
            {
                seedItems = value;
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

        #endregion

        #region 给属性赋值相关方法

        private void LoadSeedItems(XmlNode node)
        {
            foreach (XmlNode item in node.SelectNodes("item"))
            {
                LoadSeedItem(item);
            }
        }

        private void LoadSeedItem(XmlNode node)
        {
            SeedItem etSeedItem = new SeedItem();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "name":
                        etSeedItem.Name = item.InnerText;
                        break;
                    case "fruitpic":
                        etSeedItem.FruitPic = item.InnerText;
                        break;
                    case "seedid":
                        etSeedItem.SeedID = item.InnerText;
                        break;
                    case "price":
                        etSeedItem.Price = Convert.ToDouble(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            SeedItems.Add(etSeedItem);
        }

        #endregion
    }
}
