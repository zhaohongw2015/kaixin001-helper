using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SNSHelper_Win_Garden.Entity
{
    class GardenSetting
    {
        private GlobalSetting globalSetting = new GlobalSetting();
        public GlobalSetting GlobalSetting
        {
            get
            {
                return globalSetting;
            }
            set
            {
                globalSetting = value;
            }
        }

        private List<AccountSetting> accountSettings = new List<AccountSetting>();
        public List<AccountSetting> AccountSettings
        {
            get
            {
                return accountSettings;
            }
            set
            {
                accountSettings = value;
            }
        }

        #region Load

        /// <summary>
        /// 加载花园农夫配置信息
        /// </summary>
        /// <returns></returns>
        public static GardenSetting LoadGardenSetting(string filePath)
        {
            string fileName = "GardenSetting.xml";
            filePath = Path.Combine(filePath, fileName);

            if (!File.Exists(filePath))
            {
                CreateGardenSettingFile(filePath);
            }

            GardenSetting etGardenSetting = new GardenSetting();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            etGardenSetting.GlobalSetting = LoadGlobalSetting(doc.DocumentElement.SelectSingleNode("GlobalSetting"));
            etGardenSetting.AccountSettings = LoadAccountSettings(doc.DocumentElement.SelectSingleNode("AccountSettings"));

            return etGardenSetting;
        }

        private static void CreateGardenSettingFile(string filePath)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><Settings><GlobalSetting><WorkingInterval>60</WorkingInterval><NetworkDelay>3000</NetworkDelay></GlobalSetting><AccountSettings></AccountSettings></Settings>";

            StreamWriter sw = File.CreateText(filePath);
            sw.Write(xml);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        private static GlobalSetting LoadGlobalSetting(XmlNode node)
        {
            GlobalSetting etGlobalSetting = new GlobalSetting();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "WorkingInterval":
                        etGlobalSetting.WorkingInterval = Convert.ToInt32(item.InnerText);
                        break;
                    case "NetworkDelay":
                        etGlobalSetting.NetworkDelay = Convert.ToInt32(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return etGlobalSetting;
        }

        private static List<AccountSetting> LoadAccountSettings(XmlNode node)
        {
            List<AccountSetting> accountSettings = new List<AccountSetting>();

            foreach (XmlNode item in node.SelectNodes("AccountSetting"))
            {
                accountSettings.Add(LoadAccountSetting(item));
            }

            return accountSettings;
        }

        private static AccountSetting LoadAccountSetting(XmlNode node)
        {
            AccountSetting etAccountSetting = new AccountSetting();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "LoginEmail":
                        etAccountSetting.LoginEmail = item.InnerText;
                        break;
                    case "LoginPassword":
                        etAccountSetting.LoginPassword = item.InnerText;
                        break;
                    case "IsOperate":
                        etAccountSetting.IsOperate = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "AutoFarm":
                        etAccountSetting.AutoFarm = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Crops":
                        etAccountSetting.Crops = item.InnerText;
                        break;
                    case "AutoWater":
                        etAccountSetting.AutoWater = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "WaterLowLimit":
                        etAccountSetting.WaterLowLimit = item.InnerText;
                        break;
                    case "AutoVermin":
                        etAccountSetting.AutoVermin = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "AutoPlough":
                        etAccountSetting.AutoPlough = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "AutoBuySeed":
                        etAccountSetting.AutoBuySeed = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Seed":
                        etAccountSetting.Seed = item.InnerText;
                        break;
                    case "AutoHavest":
                        etAccountSetting.AutoHavest = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "AutoHavestInTime":
                        etAccountSetting.AutoHavestInTime = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "FriendWaterLowLimit":
                        etAccountSetting.FriendWaterLowLimit = item.InnerText;
                        break;
                    case "AutoGrass":
                        etAccountSetting.AutoGrass = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "AutoSell":
                        etAccountSetting.AutoSell = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "IsUsingPrivateSetting":
                        etAccountSetting.IsUsingPrivateSetting = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "StealCrops":
                        etAccountSetting.StealCrops = item.InnerText;
                        break;
                    case "FriendSettings":
                        etAccountSetting.FriendSettings = LoadFriendSettings(item);
                        break;
                    default:
                        break;
                }
            }

            return etAccountSetting;
        }

        private static List<FriendSetting> LoadFriendSettings(XmlNode node)
        {
            List<FriendSetting> friendSettings = new List<FriendSetting>();

            foreach (XmlNode item in node.SelectNodes("FriendSetting"))
            {
                friendSettings.Add(LoadFriendSetting(item));
            }

            return friendSettings;
        }

        private static FriendSetting LoadFriendSetting(XmlNode node)
        {
            FriendSetting etFriendSetting = new FriendSetting();

            foreach (XmlNode item in node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "UID":
                        etFriendSetting.UID = item.InnerText;
                        break;
                    case "Name":
                        etFriendSetting.Name = item.InnerText;
                        break;
                    case "Plough":
                        etFriendSetting.Plough = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Water":
                        etFriendSetting.Water = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Farm":
                        etFriendSetting.Farm = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Vermin":
                        etFriendSetting.Vermin = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Steal":
                        etFriendSetting.Steal = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    case "Grass":
                        etFriendSetting.Grass = Convert.ToBoolean(Convert.ToInt32(item.InnerText));
                        break;
                    default:
                        break;
                }
            }

            return etFriendSetting;
        }
        #endregion

        #region Save
        /// <summary>
        /// 保持花园农夫配置信息
        /// </summary>
        /// <param name="filePath">配置文件保存路径</param>
        /// <param name="gardenSetting"></param>
        /// <returns></returns>
        public static bool SaveGardenSetting(string filePath, GardenSetting gardenSetting)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><Settings><GlobalSetting></GlobalSetting><AccountSettings></AccountSettings></Settings>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            SaveGlobalSetting(doc.DocumentElement.SelectSingleNode("GlobalSetting"), gardenSetting.GlobalSetting);
            SaveAccountSettings(doc.DocumentElement.SelectSingleNode("AccountSettings"), gardenSetting.AccountSettings);

            try
            {
                doc.Save(Path.Combine(filePath, "GardenSetting.xml"));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static void SaveGlobalSetting(XmlNode node, GlobalSetting globalSetting)
        {
            node.AppendChild(CreateElement(node.OwnerDocument, "WorkingInterval", globalSetting.WorkingInterval.ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "NetworkDelay", globalSetting.NetworkDelay.ToString()));
        }

        private static void SaveAccountSettings(XmlNode node, List<AccountSetting> accountSettings)
        {
            foreach (AccountSetting item in accountSettings)
            {
                XmlNode newAccountSettingNode = node.OwnerDocument.CreateElement("AccountSetting");
                SaveAccountSetting(newAccountSettingNode, item);
                node.AppendChild(newAccountSettingNode);
            }
        }

        private static void SaveAccountSetting(XmlNode node, AccountSetting accountSetting)
        {
            node.AppendChild(CreateElement(node.OwnerDocument, "LoginEmail", accountSetting.LoginEmail));
            node.AppendChild(CreateElement(node.OwnerDocument, "LoginPassword", accountSetting.LoginPassword));
            node.AppendChild(CreateElement(node.OwnerDocument, "IsOperate", Convert.ToInt32(accountSetting.IsOperate).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoFarm", Convert.ToInt32(accountSetting.AutoFarm).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Crops", accountSetting.Crops));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoWater", Convert.ToInt32(accountSetting.AutoWater).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "WaterLowLimit", accountSetting.WaterLowLimit));
            node.AppendChild(CreateElement(node.OwnerDocument, "FriendWaterLowLimit", accountSetting.FriendWaterLowLimit));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoVermin", Convert.ToInt32(accountSetting.AutoVermin).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoPlough", Convert.ToInt32(accountSetting.AutoPlough).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoBuySeed", Convert.ToInt32(accountSetting.AutoBuySeed).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Seed", accountSetting.Seed));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoHavest", Convert.ToInt32(accountSetting.AutoHavest).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoHavestInTime", Convert.ToInt32(accountSetting.AutoHavestInTime).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoGrass", Convert.ToInt32(accountSetting.AutoGrass).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "AutoSell", Convert.ToInt32(accountSetting.AutoSell).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "IsUsingPrivateSetting", Convert.ToInt32(accountSetting.IsUsingPrivateSetting).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "StealCrops", accountSetting.StealCrops));

            XmlNode newFriendSettingsNode = node.OwnerDocument.CreateElement("FriendSettings");
            SaveFriendSettings(newFriendSettingsNode, accountSetting.FriendSettings);
            node.AppendChild(newFriendSettingsNode);
        }

        private static void SaveFriendSettings(XmlNode node, List<FriendSetting> friendSettings)
        {
            foreach (FriendSetting item in friendSettings)
            {
                XmlNode newFriendSettingNode = node.OwnerDocument.CreateElement("FriendSetting");
                SaveFriendSetting(newFriendSettingNode, item);
                node.AppendChild(newFriendSettingNode);
            }
        }

        private static void SaveFriendSetting(XmlNode node, FriendSetting friendSetting)
        {
            node.AppendChild(CreateElement(node.OwnerDocument, "UID", friendSetting.UID));
            node.AppendChild(CreateElement(node.OwnerDocument, "Name", friendSetting.Name));
            node.AppendChild(CreateElement(node.OwnerDocument, "Plough", Convert.ToInt32(friendSetting.Plough).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Water", Convert.ToInt32(friendSetting.Water).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Farm", Convert.ToInt32(friendSetting.Farm).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Vermin", Convert.ToInt32(friendSetting.Vermin).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Steal", Convert.ToInt32(friendSetting.Steal).ToString()));
            node.AppendChild(CreateElement(node.OwnerDocument, "Grass", Convert.ToInt32(friendSetting.Grass).ToString()));
        }

        private static XmlElement CreateElement(XmlDocument doc, string elementName, string innerText)
        {
            XmlElement xmlElement = doc.CreateElement(elementName);
            xmlElement.InnerText = innerText;

            return xmlElement;
        }

        #endregion
    }
}
