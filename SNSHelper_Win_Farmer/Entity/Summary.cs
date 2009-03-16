using System;
using System.Collections.Generic;
using System.Text;

namespace SNSHelper_Win_Garden.Entity
{
    public class Summary
    {
        public Summary()
        {
        }

        public Summary(string name)
        {
            this.name = name;
        }

        private string name = string.Empty;
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

        private int stealTimes = 0;
        public int StealTimes
        {
            get
            {
                return stealTimes;
            }
            set
            {
                stealTimes = value;
            }
        }

        private int stealedCropsNo = 0;
        public int StealedCropsNo
        {
            get
            {
                return stealedCropsNo;
            }
            set
            {
                stealedCropsNo = value;
            }
        }

        private int waterTimes = 0;
        public int WaterTimes
        {
            get
            {
                return waterTimes;
            }
            set
            {
                waterTimes = value;
            }
        }

        private int verminTimes = 0;
        public int VerminTimes
        {
            get
            {
                return verminTimes;
            }
            set
            {
                verminTimes = value;
            }
        }

        private int grassTimes = 0;
        public int GrassTimes
        {
            get
            {
                return grassTimes;
            }
            set
            {
                grassTimes = value;
            }
        }
    }
}
