using System;
using System.Collections.Generic;
using System.Xml;
using SNSHelper_Win.Entity;
using System.IO;
using System.Windows.Forms;

namespace SNSHelper_Win.Helper
{
    public class ConfigHelper
    {
        #region Property

        static GlobalSetting globalSetting = new GlobalSetting();
        static public GlobalSetting GlobalSetting
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

        static List<AccountSetting> accountSettingList = new List<AccountSetting>();
        public static List<AccountSetting> AccountSettings
        {
            get
            {
                return accountSettingList;
            }
            set
            {
                accountSettingList = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 加载配置信息
        /// </summary>
        public static void LoadConfig()
        {
            globalSetting = new GlobalSetting();
            accountSettingList = new List<AccountSetting>();

            if (!File.Exists(Path.Combine(Application.StartupPath, "data.xml")))
            {
                globalSetting.ParkInterval = 60;
                globalSetting.NetworkDelay = 3000;
                SaveConfig();
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("data.xml");
            LoadGlobalSetting(xmlDoc.DocumentElement);
            LoadAccountSettings(xmlDoc.DocumentElement);
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        public static bool SaveConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><Settings><GlobalSetting><ParkInterval></ParkInterval><NetworkDelay></NetworkDelay></GlobalSetting><AccountSettings>  </AccountSettings></Settings>");

            AddGlobalSetting(xmlDoc.DocumentElement);
            AddAccountSettings(xmlDoc.DocumentElement);

            try
            {
                xmlDoc.Save("data.xml");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static AccountSetting GetAccountByLoginEmail(string loginEmail)
        {
            for (int i = 0; i < accountSettingList.Count; i++)
            {
                if (accountSettingList[i].LoginEmail.Equals(loginEmail))
                {
                    return accountSettingList[i];
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        #region Method for loading

        private static void LoadGlobalSetting(XmlElement xmlEelenemt)
        {
            XmlNode globalSettingNode = xmlEelenemt.SelectSingleNode("GlobalSetting");

            globalSetting.ParkInterval = Convert.ToInt32(globalSettingNode["ParkInterval"].InnerText);
            globalSetting.NetworkDelay = Convert.ToInt32(globalSettingNode["NetworkDelay"].InnerText);
        }

        private static void LoadAccountSettings(XmlElement xmlEelenemt)
        {
            XmlNode accountSettingsNode = xmlEelenemt.SelectSingleNode("AccountSettings");

            for (int i = 0; i < accountSettingsNode.ChildNodes.Count; i++)
            {
                LoadAccountSetting(accountSettingsNode.ChildNodes[i]);
            }
        }

        private static void LoadAccountSetting(XmlNode node)
        {
            AccountSetting accountSetting = new AccountSetting();
            accountSetting.LoginEmail = node.Attributes["loginEmail"].Value;
            accountSetting.LoginPwd = node.Attributes["loginPwd"].Value;
            accountSetting.IsOperation = Convert.ToBoolean(node["Operation"].InnerText);
            accountSetting.MasterAccount = node["MasterAccount"].InnerText;
            accountSetting.MinPark = Convert.ToInt32(node["MinPark"].InnerText);
            accountSetting.MinPost = Convert.ToInt32(node["MinPost"].InnerText);
            accountSetting.FriendSettings = LoadFriendSettingsForAccount(node["FriendSettings"]);

            accountSettingList.Add(accountSetting);
        }

        private static List<FriendSetting> LoadFriendSettingsForAccount(XmlNode node)
        {
            List<FriendSetting> list = new List<FriendSetting>();

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                list.Add(LoadFriendSetting(node.ChildNodes[i]));
            }

            return list;
        }

        private static FriendSetting LoadFriendSetting(XmlNode node)
        {
            FriendSetting friendSetting = new FriendSetting();
            friendSetting.UID = node["UID"].InnerText;
            friendSetting.NickName = node["NickName"].InnerText;
            friendSetting.AllowedPark = Convert.ToBoolean(node["AllowedPark"].InnerText);
            friendSetting.AllowedPost = Convert.ToBoolean(node["AllowedPost"].InnerText);
            friendSetting.ParkPriority = string.IsNullOrEmpty(node["ParkPriority"].InnerText) ? 0 : Convert.ToInt32(node["ParkPriority"].InnerText);

            return friendSetting;
        }

        #endregion

        #region Method for saving

        private static XmlElement CreateElement(XmlDocument doc, string elementName, string innerText)
        {
            XmlElement xmlElement = doc.CreateElement(elementName);
            xmlElement.InnerText = innerText;

            return xmlElement;
        }

        private static void AddGlobalSetting(XmlElement xmlElement)
        {
            XmlNode globalSettingNode = xmlElement.SelectSingleNode("GlobalSetting");
            globalSettingNode["ParkInterval"].InnerText = globalSetting.ParkInterval.ToString();
            globalSettingNode["NetworkDelay"].InnerText = globalSetting.NetworkDelay.ToString();
        }

        private static void AddAccountSettings(XmlElement xmlElement)
        {
            XmlNode accountSettingsNode = xmlElement.SelectSingleNode("AccountSettings");

            for (int i = 0; i < accountSettingList.Count; i++)
            {
                XmlElement xeAccountSetting = xmlElement.OwnerDocument.CreateElement("AccountSetting");
                xeAccountSetting.SetAttribute("loginEmail", accountSettingList[i].LoginEmail);
                xeAccountSetting.SetAttribute("loginPwd", accountSettingList[i].LoginPwd);
                xeAccountSetting.AppendChild(CreateElement(xeAccountSetting.OwnerDocument, "Operation", accountSettingList[i].IsOperation.ToString()));
                xeAccountSetting.AppendChild(CreateElement(xeAccountSetting.OwnerDocument, "MasterAccount", accountSettingList[i].MasterAccount));
                xeAccountSetting.AppendChild(CreateElement(xeAccountSetting.OwnerDocument, "MinPark", accountSettingList[i].MinPark.ToString()));
                xeAccountSetting.AppendChild(CreateElement(xeAccountSetting.OwnerDocument, "MinPost", accountSettingList[i].MinPost.ToString()));

                XmlElement xeFriendSettings = xeAccountSetting.OwnerDocument.CreateElement("FriendSettings");
                AddFriendSettings(xeFriendSettings, accountSettingList[i].FriendSettings);
                xeAccountSetting.AppendChild(xeFriendSettings);

                accountSettingsNode.AppendChild(xeAccountSetting);
            }
        }

        private static void AddFriendSettings(XmlElement friendSettingsElement, List<FriendSetting> friendSettingList)
        {
            friendSettingList = ShortFriendSettings(friendSettingList);

            for (int i = 0; i < friendSettingList.Count; i++)
            {
                XmlElement friendElement = friendSettingsElement.OwnerDocument.CreateElement("Friend");
                friendElement.AppendChild(CreateElement(friendSettingsElement.OwnerDocument, "UID", friendSettingList[i].UID));
                friendElement.AppendChild(CreateElement(friendSettingsElement.OwnerDocument, "NickName", friendSettingList[i].NickName));
                friendElement.AppendChild(CreateElement(friendSettingsElement.OwnerDocument, "AllowedPark", friendSettingList[i].AllowedPark.ToString()));
                friendElement.AppendChild(CreateElement(friendSettingsElement.OwnerDocument, "AllowedPost", friendSettingList[i].AllowedPost.ToString()));
                friendElement.AppendChild(CreateElement(friendSettingsElement.OwnerDocument, "ParkPriority", friendSettingList[i].ParkPriority.ToString()));

                friendSettingsElement.AppendChild(friendElement);
            }
        }

        private static List<FriendSetting> ShortFriendSettings(List<FriendSetting> friendSettingList)
        {
            if (friendSettingList == null || friendSettingList.Count == 0)
            {
                return friendSettingList;
            }

            FriendSetting temp;
            bool exchange = false;

            do
            {
                exchange = false;

                for (int i = friendSettingList.Count - 1; i > 0; i--)
                {
                    if (friendSettingList[i].ParkPriority < friendSettingList[i - 1].ParkPriority)
                    {
                        temp = friendSettingList[i - 1];
                        friendSettingList[i - 1] = friendSettingList[i];
                        friendSettingList[i] = temp;

                        exchange = true;
                    }
                }
            } while (exchange);

            return friendSettingList;
        }
        #endregion

        #endregion
    }
}
