using System;
using System.Collections.Generic;
using System.Xml;
using SNSHelper_Win_Parker.Entity;

namespace SNSHelper_Win_Parker.Helper
{
    public class CarMarketHelper
    {
        #region Properties

        private static List<Car> carList = new List<Car>();
        public static List<Car> CarList
        {
            get
            {
                return carList;
            }
            set
            {
                carList = value;
            }
        }

        #endregion

        public static void LoadCarList()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("CarMarket.xml");

            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                carList.Add(LoadCar(doc.DocumentElement.ChildNodes[i]));
            }
        }

        private static Car LoadCar(XmlNode node)
        {
            Car etCar = new Car();
            etCar.ID = node.Attributes["ID"].Value;
            etCar.Name = node.Attributes["Name"].Value;
            etCar.Price = Convert.ToInt32(node.Attributes["Price"].Value);

            return etCar;
        }
    }
}
