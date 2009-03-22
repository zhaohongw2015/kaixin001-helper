using System;
using System.Collections.Generic;
using System.Text;
using SNSHelper.Kaixin001.Entity.Garden;
using System.Xml;
using System.IO;

namespace SNSHelper_Win_Garden.Helper
{
    public class SeedDataHelper
    {
        public static bool SaveSeedData(string filePath, SeedData seedData)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><data><seed></seed><ret>succ</ret></data>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode seedNode = doc.DocumentElement.SelectSingleNode("seed");
            foreach (SeedItem item in seedData.SeedItems)
            {
                SaveSeedItem(seedNode, item);
            }

            try
            {
                doc.Save(Path.Combine(filePath, "SeedData.xml"));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static void SaveSeedItem(XmlNode node, SeedItem seedItem)
        {
            XmlNode itemNode = node.OwnerDocument.CreateElement("item");
            itemNode.AppendChild(CreateElement(node.OwnerDocument, "name", seedItem.Name));
            itemNode.AppendChild(CreateElement(node.OwnerDocument, "fruitpic", seedItem.FruitPic));
            itemNode.AppendChild(CreateElement(node.OwnerDocument, "seedid", seedItem.SeedID));
            itemNode.AppendChild(CreateElement(node.OwnerDocument, "price", seedItem.Price.ToString()));

            node.AppendChild(itemNode);
        }

        private static XmlElement CreateElement(XmlDocument doc, string elementName, string innerText)
        {
            XmlElement xmlElement = doc.CreateElement(elementName);
            xmlElement.InnerText = innerText;

            return xmlElement;
        }
    }
}
