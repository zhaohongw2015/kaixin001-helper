using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper.Kaixin001.Entity.Parking
{
    public class ParkerStaInfo : EntityBase
    {
        /// <summary>
        /// 用户id
        /// </summary>
        string uid;
        public string UId
        {
            get
            {
                return uid;
            }
            set
            {
                uid = value;
            }
        }

        
        /// <summary>
        /// 用户姓名
        /// </summary>
        string real_name;
        public string RealName
        {
            get
            {
                return real_name;
            }
            set
            {
                real_name = value;
 
            }
        }

        /// <summary>
        /// 用户现金
        /// </summary>
        double cashcount;

        public double Cashcount
        {
            get {
                return cashcount; 
            }
            set { 
                cashcount = value;
            }
        }

        /// <summary>
        /// 汽车总数
        /// </summary>
        int carcount;

        public int Carcount
        {
            get { 
                return carcount; 
            }
            set { 
                carcount = value; 
            }
        }

        double carCountPrice;

        public double CarCountPrice
        {
            get { return carCountPrice; }
            set { carCountPrice = value; }
        }

    }
}
