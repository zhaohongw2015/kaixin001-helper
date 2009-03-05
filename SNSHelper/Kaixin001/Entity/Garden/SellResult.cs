using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class SellResult
    {
        public SellResult()
        {
        }

        public SellResult(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"gb2312\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                errMsg = "错误编号：006．" + e.Message;
                return;
            }

            LoadSellResult(doc.DocumentElement);
        }

        private void LoadSellResult(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ret":
                        ret = item.InnerText;
                        break;
                    case "goodsname":
                        goodsName = item.InnerText;
                        break;
                    case "totalprice":
                        totalPrice = Convert.ToDouble(item.InnerText);
                        break;
                    case "num":
                        num = item.InnerText;
                        break;
                    case "pic":
                        pic = item.InnerText;
                        break;
                    case "all":
                        isAll = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    default:
                        break;
                }
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

        #region 属性

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

        private string goodsName = string.Empty;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName
        {
            get
            {
                return goodsName;
            }
            set
            {
                goodsName = value;
            }
        }

        private double totalPrice = 0;
        /// <summary>
        /// 出售商品总价
        /// </summary>
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

        private string num = string.Empty;
        /// <summary>
        /// 出售数量
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

        private string pic = string.Empty;
        /// <summary>
        /// 出售商品图片
        /// </summary>
        public string Pic
        {
            get
            {
                return pic;
            }
            set
            {
                pic = value;
            }
        }

        private bool isAll = false;
        /// <summary>
        /// 是否全部出售
        /// </summary>
        public bool IsAll
        {
            get
            {
                return isAll;
            }
            set
            {
                isAll = value;
            }
        } 

        #endregion
    }
}
