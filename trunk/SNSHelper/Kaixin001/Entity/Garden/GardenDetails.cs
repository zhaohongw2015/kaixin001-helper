using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SNSHelper.Common;
using System.Text.RegularExpressions;

namespace SNSHelper.Kaixin001.Entity.Garden
{
    public class GardenDetails
    {
        public GardenDetails()
        {
        }

        public GardenDetails(string xml)
        {
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + xml;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception e)
            {
                try
                {
                    string parten = "<setting>(.*?)</setting>";
                    xml = Regex.Replace(xml, parten, "<setting><water></water><vermin></vermin><steal></steal><farm></farm></setting>");
                    doc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    errMsg = "错误编号：001．" + ex.Message;

                    if (xml.Contains("你不是她好友，没有权限查看此内容"))
                    {
                        errMsg = "1";
                    }

                    return;
                }
            }

            if (doc.DocumentElement.SelectSingleNode("account") != null)
            {
                LoadAccount(doc.DocumentElement.SelectSingleNode("account"));
            }

            if (doc.DocumentElement.SelectSingleNode("garden") != null)
            {
                LoadGardenItems(doc.DocumentElement.SelectSingleNode("garden"));
            }

            if (doc.DocumentElement.SelectSingleNode("ret") != null)
            {
                ret = doc.DocumentElement.SelectSingleNode("ret").InnerText;
            }
        }

        #region 属性

        private Account account = new Account();
        public Account Account
        {
            get
            {
                return account;
            }
            set
            {
                account = value;
            }
        }

        private List<GardenItem> gardenItems = new List<GardenItem>();
        public List<GardenItem> GarderItems
        {
            get
            {
                return gardenItems;
            }
            set
            {
                gardenItems = value;
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

        private void LoadAccount(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "rank":
                        account.Rank = item.InnerText;
                        break;
                    case "ranktip":
                        account.RankTip = item.InnerText;
                        break;
                    case "cash":
                        account.Cash = item.InnerText;
                        break;
                    case "logo50":
                        account.Logo50 = item.InnerText;
                        break;
                    case "name":
                        account.Name = item.InnerText;
                        break;
                    case "cashtip":
                        account.CashTip = item.InnerText;
                        break;
                    case "bkswf":
                        account.BkSwf = item.InnerText;
                        break;
                    case "bfirst":
                        account.BFirst = item.InnerText;
                        break;
                    case "tcharms":
                        account.TCharms = item.InnerText;
                        break;
                    case "careurl":
                        account.CareUrl = item.InnerText;
                        break;
                    case "setting":
                        LoadSetting(item);
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadSetting(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "water":
                        account.Setting.Water = item.InnerText;
                        break;
                    case "vermin":
                        account.Setting.Vermin = item.InnerText;
                        break;
                    case "steal":
                        account.Setting.Steal = item.InnerText;
                        break;
                    case "farm":
                        account.Setting.Farm = item.InnerText;
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadGardenItems(XmlNode node)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                LoadGardenItem(item);
            }
        }

        private void LoadGardenItem(XmlNode node)
        {
            GardenItem etItem = new GardenItem();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "water":
                        etItem.Water = Convert.ToInt32(item.InnerText);
                        break;
                    case "farmnum":
                        etItem.FarmNum = item.InnerText;
                        break;
                    case "vermin":
                        etItem.Vermin = Convert.ToInt32(item.InnerText);
                        break;
                    case "cropsid":
                        etItem.CropsId = item.InnerText;
                        break;
                    case "fuid":
                        etItem.FUId = item.InnerText;
                        break;
                    case "status":
                        etItem.Status = item.InnerText;
                        break;
                    case "shared":
                        etItem.Shared = item.InnerText;
                        break;
                    case "pic":
                        etItem.Pic = item.InnerText;
                        break;
                    case "fruitpic":
                        etItem.FruitPic = item.InnerText;
                        break;
                    case "picwidth":
                        etItem.PicWidth = item.InnerText;
                        break;
                    case "picheight":
                        etItem.PicHeight = item.InnerText;
                        break;
                    case "cropsstatus":
                        etItem.CropsStatus = item.InnerText;
                        break;
                    case "grow":
                        etItem.Grow = item.InnerText;
                        break;
                    case "totalgrow":
                        etItem.TotalGrow = item.InnerText;
                        break;
                    case "fruitnum":
                        etItem.FruitNum = item.InnerText;
                        break;
                    case "seedid":
                        etItem.SeedId = item.InnerText;
                        break;
                    case "crops":
                        etItem.Crops = item.InnerText;
                        break;
                    case "farm":
                        etItem.Farm = item.InnerText;
                        break;
                    case "grass":
                        etItem.Grass = item.InnerText;
                        break;
                    default:
                        break;
                }
            }

            gardenItems.Add(etItem);
        }
        #endregion
    }
}
