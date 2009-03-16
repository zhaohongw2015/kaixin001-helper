using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper_Win_Garden.Entity
{
    public class CropsIncome
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private int growthCycle = 0;
        public int GrowthCycle
        {
            get
            {
                return growthCycle;
            }
            set
            {
                growthCycle = value;
            }
        }

        private int unitPrice = 0;
        public int UnitPrice
        {
            get
            {
                return unitPrice;
            }
            set
            {
                unitPrice = value;
            }
        }

        private int theftproof = 0;
        public int Theftproof
        {
            get
            {
                return theftproof;
            }
            set
            {
                theftproof = value;
            }
        }
    }
}
