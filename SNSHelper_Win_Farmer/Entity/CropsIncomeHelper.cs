using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SNSHelper_Win_Garden.Entity
{
    public class CropsIncomeHelper
    {
        private static List<CropsIncome> cropsIncomeList;
        public static List<CropsIncome> CropsIncomeList
        {
            get
            {
                if (cropsIncomeList == null)
                {
                    LoadCropsIncomeList();
                }

                return cropsIncomeList;
            }
            set
            {
                cropsIncomeList = value;
            }
        }

        public static CropsIncome GetCropsIncome(string name)
        {
            foreach (CropsIncome item in CropsIncomeList)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }

            return new CropsIncome();
        }

        private static void LoadCropsIncomeList()
        {
            string filePath = Path.Combine(Application.StartupPath, "CropsIncome.xml");
            if (!File.Exists(filePath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            cropsIncomeList = new List<CropsIncome>();
            foreach (XmlNode item1 in doc.DocumentElement.ChildNodes)
            {
                CropsIncome cropsIncome = new CropsIncome();
                foreach (XmlNode item in item1.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "Name":
                            cropsIncome.Name = item.InnerText;
                            break;
                        case "GrowthCycle":
                            cropsIncome.GrowthCycle = Convert.ToInt32(item.InnerText);
                            break;
                        case "UnitPrice":
                            cropsIncome.UnitPrice = Convert.ToInt32(item.InnerText);
                            break;
                        case "Theftproof":
                            cropsIncome.Theftproof = Convert.ToInt32(item.InnerText);
                            break;
                        default:
                            break;
                    }
                }

                cropsIncomeList.Add(cropsIncome);
            }
        }

        public static bool SaveCropsIncome(string filePath, List<CropsIncome> list)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Crops></Crops>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            foreach (CropsIncome item in list)
            {
                SaveCropIncome(doc.DocumentElement, item);
            }

            try
            {
                doc.Save(Path.Combine(filePath, "CropsIncome.xml"));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static void SaveCropIncome(XmlNode node, CropsIncome item)
        {
            XmlNode cropNode = node.OwnerDocument.CreateElement("Crop");
            cropNode.AppendChild(CreateElement(node.OwnerDocument, "Name", item.Name));
            cropNode.AppendChild(CreateElement(node.OwnerDocument, "GrowthCycle", item.GrowthCycle.ToString()));
            cropNode.AppendChild(CreateElement(node.OwnerDocument, "UnitPrice", item.UnitPrice.ToString()));
            cropNode.AppendChild(CreateElement(node.OwnerDocument, "Theftproof", item.Theftproof.ToString()));

            node.AppendChild(cropNode);
        }

        private static XmlElement CreateElement(XmlDocument doc, string elementName, string innerText)
        {
            XmlElement xmlElement = doc.CreateElement(elementName);
            xmlElement.InnerText = innerText;

            return xmlElement;
        }
    }
}
