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
    }
}
