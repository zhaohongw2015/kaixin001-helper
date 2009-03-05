using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SNSHelper_Win_Garden.Entity
{
    public class SeedMVPs
    {
        private static List<SeedMVP> seedMVPList;
        public static List<SeedMVP> SeedMVPList
        {
            get
            {
                if (seedMVPList == null)
                {
                    LoadSeedMVPs();
                }

                return seedMVPList;
            }
            set
            {
                seedMVPList = value;
            }
        }

        public static string GetSeedName(string rank)
        {
            foreach (SeedMVP item in SeedMVPList)
            {
                if (item.Rank == rank)
                {
                    return item.SeedName;
                }
            }

            return string.Empty;
        }

        public static void SetSeedMVP(string rank, string seedName)
        {
            foreach (SeedMVP item in seedMVPList)
            {
                if (item.Rank == rank)
                {
                    item.SeedName = seedName;

                    return;
                }
            }

            SeedMVP etSeedMVP = new SeedMVP();
            etSeedMVP.Rank = rank;
            etSeedMVP.SeedName = seedName;
            seedMVPList.Add(etSeedMVP);
        }

        private static void LoadSeedMVPs()
        {
            string filePath = Path.Combine(Application.StartupPath, "SeedMVP.xml");
            if (!File.Exists(filePath))
            {
                CreateSeedMVPFile(filePath);
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            seedMVPList = new List<SeedMVP>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                SeedMVP etSeedMVP = new SeedMVP();
                foreach (XmlNode item in node.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "Rank":
                            etSeedMVP.Rank = item.InnerText;
                            break;
                        case "Seed":
                            etSeedMVP.SeedName = item.InnerText;
                            break;
                        default:
                            break;
                    }
                }
                seedMVPList.Add(etSeedMVP);
            }
        }

        private static void CreateSeedMVPFile(string filePath)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><SeedMVPs></SeedMVPs>";

            StreamWriter sw = File.CreateText(filePath);
            sw.Write(xml);
            sw.Flush();
            sw.Close();
            sw.Dispose();

            seedMVPList = new List<SeedMVP>();

            return;
        }

        public static bool SaveSeedMVPs()
        {
            string filePath = Path.Combine(Application.StartupPath, "SeedMVP.xml");
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><SeedMVPs></SeedMVPs>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            foreach (SeedMVP seedMVP in seedMVPList)
            {
                XmlElement seedMVPElement = doc.CreateElement("SeedMVP");

                seedMVPElement.AppendChild(CreateElement(doc, "Rank", seedMVP.Rank));
                seedMVPElement.AppendChild(CreateElement(doc, "Seed", seedMVP.SeedName));

                doc.DocumentElement.AppendChild(seedMVPElement);
            }

            try
            {
                doc.Save(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static XmlElement CreateElement(XmlDocument doc, string elementName, string innerText)
        {
            XmlElement xmlElement = doc.CreateElement(elementName);
            xmlElement.InnerText = innerText;

            return xmlElement;
        }

    }
}
