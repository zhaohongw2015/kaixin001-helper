using System.Collections.Generic;

namespace SNSHelper_Win_Garden.Entity
{
    class AccountSetting
    {
        private string loginEmail = string.Empty;
        public string LoginEmail
        {
            get
            {
                return loginEmail;
            }
            set
            {
                loginEmail = value;
            }
        }

        private string loginPassword = string.Empty;
        public string LoginPassword
        {
            get
            {
                return loginPassword;
            }
            set
            {
                loginPassword = value;
            }
        }

        private bool isOperate = false;
        public bool IsOperate
        {
            get
            {
                return isOperate;
            }
            set
            {
                isOperate = value;
            }
        }

        private bool autoFarm = false;
        public bool AutoFarm
        {
            get
            {
                return autoFarm;
            }
            set
            {
                autoFarm = value;
            }
        }

        private string crops = string.Empty;
        public string Crops
        {
            get
            {
                return crops;
            }
            set
            {
                crops = value;
            }
        }

        private bool autoWater = false;
        public bool AutoWater
        {
            get
            {
                return autoWater;
            }
            set
            {
                autoWater = value;
            }
        }

        private string waterLowLimit = string.Empty;
        public string WaterLowLimit
        {
            get
            {
                return waterLowLimit;
            }
            set
            {
                waterLowLimit = value;
            }
        }

        private bool autoVermin = false;
        public bool AutoVermin
        {
            get
            {
                return autoVermin;
            }
            set
            {
                autoVermin = value;
            }
        }

        private bool autoPlough = false;
        public bool AutoPlough
        {
            get
            {
                return autoPlough;
            }
            set
            {
                autoPlough = value;
            }
        }

        private bool autoBuySeed = false;
        public bool AutoBuySeed
        {
            get
            {
                return autoBuySeed;
            }
            set
            {
                autoBuySeed = value;
            }
        }

        private string seed = string.Empty;
        public string Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;
            }
        }

        private bool autoHavest = false;
        public bool AutoHavest
        {
            get
            {
                return autoHavest;
            }
            set
            {
                autoHavest = value;
            }
        }

        private string friendWaterLowLimit = string.Empty;
        public string FriendWaterLowLimit
        {
            get
            {
                return friendWaterLowLimit;
            }
            set
            {
                friendWaterLowLimit = value;
            }
        }

        private bool autoGrass = false;
        public bool AutoGrass
        {
            get
            {
                return autoGrass;
            }
            set
            {
                autoGrass = value;
            }
        }

        private bool autoSell = false;
        public bool AutoSell
        {
            get
            {
                return autoSell;
            }
            set
            {
                autoSell = value;
            }
        }

        private bool isUsingPrivateSetting = false;
        public bool IsUsingPrivateSetting
        {
            get
            {
                return isUsingPrivateSetting;
            }
            set
            {
                isUsingPrivateSetting = value;
            }
        }

        private string stealCorps = string.Empty;
        public string StealCrops
        {
            get
            {
                return stealCorps;
            }
            set
            {
                stealCorps = value;
            }
        }

        private bool autoHavestInTime = false;
        public bool AutoHavestInTime
        {
            get
            {
                return autoHavestInTime;
            }
            set
            {
                autoHavestInTime = value;
            }
        }

        private List<FriendSetting> friendSettings = new List<FriendSetting>();
        public List<FriendSetting> FriendSettings
        {
            get
            {
                return friendSettings;
            }
            set
            {
                friendSettings = value;
            }
        }
    }
}
