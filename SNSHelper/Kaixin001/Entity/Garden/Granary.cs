using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class Granary
    {
        public Granary()
        {
        }

        public Granary(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"gb2312\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            if (doc.DocumentElement.SelectSingleNode("fruit") != null)
            {
                LoadFruitItems(doc.DocumentElement.SelectSingleNode("fruit"));
            }

            if (doc.DocumentElement.SelectSingleNode("ret") != null)
            {
                ret = doc.DocumentElement.SelectSingleNode("ret").InnerText;
            }

            if (doc.DocumentElement.SelectSingleNode("totalprice") != null)
            {
                totalPrice = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("totalprice").InnerText);
            }
        }

        #region 属性

        private List<FruitItem> fruitItems = new List<FruitItem>();
        public List<FruitItem> FruitItems
        {
            get
            {
                return fruitItems;
            }
            set
            {
                fruitItems = value;
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

        private double totalPrice = 0;
        public double TotalPrice
        {
            get
            {
                return totalPrice;
            }
            set
            {
                totalPrice = value;
            }
        }

        #endregion

        #region 给属性赋值相关方法

        private void LoadFruitItems(XmlNode node)
        {
            foreach (XmlNode item in node.SelectNodes("item"))
            {
                LoadFruitItem(item);
            }
        }

        private void LoadFruitItem(XmlNode node)
        {
            FruitItem etFruitItem = new FruitItem();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "seedid":
                        etFruitItem.SeedID = item.InnerText;
                        break;
                    case "num":
                        etFruitItem.Num = item.InnerText;
                        break;
                    case "fruitpic":
                        etFruitItem.FruitPic = item.InnerText;
                        break;
                    case "name":
                        etFruitItem.Name = item.InnerText;
                        break;
                    default:
                        break;
                }
            }

            fruitItems.Add(etFruitItem);
        }

        #endregion
    }
}
